using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MedAI.Application.Common;
using MedAI.Application.DTOs.Clinical;
using MedAI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedAI.API.Controllers;

[Authorize]
[ApiController]
[Route("api/patients")]
[Produces("application/json")]
public class PatientsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public PatientsController(IApplicationDbContext context)
    {
        _context = context;
    }

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<PatientProfileDto>>> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (patient == null) return NotFound(ApiResponse<PatientProfileDto>.Fail("Patient profile not found."));

        var dto = new PatientProfileDto
        {
            Id = patient.Id,
            UserId = patient.UserId,
            FirstName = patient.User.FirstName,
            LastName = patient.User.LastName,
            Email = patient.User.Email,
            PhoneNumber = patient.User.PhoneNumber,
            DateOfBirth = patient.User.DateOfBirth,
            Gender = patient.User.Gender,
            BloodType = patient.BloodType,
            EmergencyContactName = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone,
            Address = patient.Address,
            CreatedAt = patient.CreatedAt
        };

        return Ok(ApiResponse<PatientProfileDto>.Ok(dto));
    }

    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<PatientProfileDto>>> UpdateMyProfile([FromBody] UpdatePatientProfileDto request)
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (patient == null) return NotFound(ApiResponse<PatientProfileDto>.Fail("Patient profile not found."));

        patient.BloodType = request.BloodType;
        patient.EmergencyContactName = request.EmergencyContactName;
        patient.EmergencyContactPhone = request.EmergencyContactPhone;
        patient.Address = request.Address;
        patient.User.PhoneNumber = request.PhoneNumber;
        patient.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetMyProfile();
    }

    [HttpGet("me/health-passport")]
    public async Task<ActionResult<ApiResponse<HealthPassportDto>>> GetHealthPassport()
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles
            .Include(p => p.User)
            .Include(p => p.Medications)
            .Include(p => p.LabResults)
            .Include(p => p.HealthEvents)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (patient == null) return NotFound(ApiResponse<HealthPassportDto>.Fail("Patient profile not found."));

        int age = DateTime.UtcNow.Year - patient.User.DateOfBirth.Year;

        var dto = new HealthPassportDto
        {
            PatientId = patient.Id,
            FullName = $"{patient.User.FirstName} {patient.User.LastName}",
            DateOfBirth = patient.User.DateOfBirth,
            Age = age > 0 ? age : 30,
            BloodType = patient.BloodType,
            Gender = patient.User.Gender,
            EmergencyContactName = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone,
            ActiveMedications = patient.Medications.Select(m => new MedicationSummaryDto
            {
                Name = m.Name,
                Dosage = m.Dosage,
                Frequency = m.Frequency
            }).ToList(),
            RecentLabResults = patient.LabResults.OrderByDescending(l => l.TestDate).Take(5).Select(l => new LabSummaryDto
            {
                TestName = l.TestName,
                Value = l.Value,
                Unit = l.Unit,
                Status = l.Status,
                TestDate = l.TestDate
            }).ToList(),
            ActiveConditions = patient.HealthEvents.Select(e => new HealthEventDto
            {
                Id = e.Id,
                PatientId = e.PatientId,
                Type = e.Type,
                Title = e.Title,
                Description = e.Description,
                EventDate = e.EventDate,
                CreatedAt = e.CreatedAt
            }).ToList()
        };

        return Ok(ApiResponse<HealthPassportDto>.Ok(dto));
    }

    [HttpGet("me/timeline")]
    public async Task<ActionResult<ApiResponse<List<TimelineItemDto>>>> GetTimeline()
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<List<TimelineItemDto>>.Fail("Patient profile not found."));

        var timeline = new List<TimelineItemDto>();

        var appts = await _context.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Where(a => a.PatientId == patient.Id)
            .ToListAsync();

        foreach (var a in appts)
        {
            timeline.Add(new TimelineItemDto
            {
                Id = a.Id,
                Category = "Appointment",
                Title = $"Appointment with Dr. {a.Doctor.User.LastName}",
                Description = $"{a.Reason} ({a.Status})",
                Date = a.AppointmentDate,
                BadgeColor = "blue"
            });
        }

        var labs = await _context.LabResults.Where(l => l.PatientId == patient.Id).ToListAsync();
        foreach (var l in labs)
        {
            timeline.Add(new TimelineItemDto
            {
                Id = l.Id,
                Category = "LabResult",
                Title = $"Lab Result: {l.TestName}",
                Description = $"Value: {l.Value} {l.Unit} ({l.Status})",
                Date = l.TestDate,
                BadgeColor = l.Status == Domain.Enums.LabResultStatus.Normal ? "emerald" : "amber"
            });
        }

        var docs = await _context.MedicalDocuments.Where(d => d.PatientId == patient.Id).ToListAsync();
        foreach (var d in docs)
        {
            timeline.Add(new TimelineItemDto
            {
                Id = d.Id,
                Category = "Document",
                Title = $"Document: {d.FileName}",
                Description = d.AISummary,
                Date = d.UploadedAt,
                BadgeColor = "purple"
            });
        }

        var sorted = timeline.OrderByDescending(t => t.Date).ToList();
        return Ok(ApiResponse<List<TimelineItemDto>>.Ok(sorted));
    }

    [HttpGet("me/documents")]
    public async Task<ActionResult<ApiResponse<List<MedicalDocumentDto>>>> GetDocuments()
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<List<MedicalDocumentDto>>.Fail("Patient profile not found."));

        var docs = await _context.MedicalDocuments
            .Where(d => d.PatientId == patient.Id)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new MedicalDocumentDto
            {
                Id = d.Id,
                PatientId = d.PatientId,
                UploadedBy = d.UploadedBy,
                FileName = d.FileName,
                FileType = d.FileType,
                FileUrl = d.FileUrl,
                DocumentType = d.DocumentType,
                ExtractedText = d.ExtractedText,
                AISummary = d.AISummary,
                UploadedAt = d.UploadedAt
            }).ToListAsync();

        return Ok(ApiResponse<List<MedicalDocumentDto>>.Ok(docs));
    }

    [HttpGet("me/lab-results")]
    public async Task<ActionResult<ApiResponse<List<LabResultDto>>>> GetLabResults()
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<List<LabResultDto>>.Fail("Patient profile not found."));

        var labs = await _context.LabResults
            .Include(l => l.Doctor).ThenInclude(d => d.User)
            .Where(l => l.PatientId == patient.Id)
            .OrderByDescending(l => l.TestDate)
            .Select(l => new LabResultDto
            {
                Id = l.Id,
                PatientId = l.PatientId,
                PatientName = $"{patient.User.FirstName} {patient.User.LastName}",
                DoctorId = l.DoctorId,
                DoctorName = $"Dr. {l.Doctor.User.FirstName} {l.Doctor.User.LastName}",
                TestName = l.TestName,
                Value = l.Value,
                Unit = l.Unit,
                ReferenceRange = l.ReferenceRange,
                Status = l.Status,
                TestDate = l.TestDate,
                Notes = l.Notes
            }).ToListAsync();

        return Ok(ApiResponse<List<LabResultDto>>.Ok(labs));
    }

    [HttpGet("me/medications")]
    public async Task<ActionResult<ApiResponse<List<MedicationDto>>>> GetMedications()
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<List<MedicationDto>>.Fail("Patient profile not found."));

        var meds = await _context.Medications
            .Where(m => m.PatientId == patient.Id)
            .OrderByDescending(m => m.StartDate)
            .Select(m => new MedicationDto
            {
                Id = m.Id,
                PatientId = m.PatientId,
                Name = m.Name,
                Dosage = m.Dosage,
                Frequency = m.Frequency,
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                Notes = m.Notes,
                CreatedAt = m.CreatedAt
            }).ToListAsync();

        return Ok(ApiResponse<List<MedicationDto>>.Ok(meds));
    }

    [HttpGet("me/appointments")]
    public async Task<ActionResult<ApiResponse<List<AppointmentDto>>>> GetAppointments()
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<List<AppointmentDto>>.Fail("Patient profile not found."));

        var appts = await _context.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Clinic)
            .Where(a => a.PatientId == patient.Id)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientName = $"{patient.User.FirstName} {patient.User.LastName}",
                DoctorId = a.DoctorId,
                DoctorName = $"Dr. {a.Doctor.User.FirstName} {a.Doctor.User.LastName}",
                DoctorSpecialization = a.Doctor.Specialization,
                ClinicId = a.ClinicId,
                ClinicName = a.Clinic.Name,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime.ToString(@"hh\:mm"),
                EndTime = a.EndTime.ToString(@"hh\:mm"),
                Status = a.Status,
                Reason = a.Reason,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt
            }).ToListAsync();

        return Ok(ApiResponse<List<AppointmentDto>>.Ok(appts));
    }
}
