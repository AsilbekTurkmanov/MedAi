using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MedAI.Application.Common;
using MedAI.Application.DTOs.Clinical;
using MedAI.Application.Interfaces;
using MedAI.Domain.Entities;
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
            .Include(p => p.Allergies)
            .Include(p => p.ChronicConditions)
            .Include(p => p.Vaccinations)
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
            ActiveMedications = patient.Medications
                .Where(m => m.Status == Domain.Enums.MedicationStatus.Active)
                .Select(m => new MedicationSummaryDto
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
            }).ToList(),
            Allergies = patient.Allergies.Select(a => new AllergyDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                Name = a.Name,
                Severity = a.Severity,
                Reaction = a.Reaction,
                DiagnosedDate = a.DiagnosedDate,
                Source = a.Source,
                CreatedAt = a.CreatedAt
            }).ToList(),
            ChronicConditions = patient.ChronicConditions.Select(c => new ChronicConditionDto
            {
                Id = c.Id,
                PatientId = c.PatientId,
                Name = c.Name,
                DiagnosedDate = c.DiagnosedDate,
                Status = c.Status,
                Notes = c.Notes,
                Source = c.Source,
                CreatedAt = c.CreatedAt
            }).ToList(),
            Vaccinations = patient.Vaccinations.Select(v => new VaccinationDto
            {
                Id = v.Id,
                PatientId = v.PatientId,
                Name = v.Name,
                DateAdministered = v.DateAdministered,
                Provider = v.Provider,
                DoseNumber = v.DoseNumber,
                Notes = v.Notes,
                CreatedAt = v.CreatedAt
            }).ToList()
        };

        return Ok(ApiResponse<HealthPassportDto>.Ok(dto));
    }

    [HttpGet("me/allergies")]
    public async Task<ActionResult<ApiResponse<List<AllergyDto>>>> GetMyAllergies()
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<List<AllergyDto>>.Fail("Patient profile not found."));

        var allergies = await _context.Allergies
            .Where(a => a.PatientId == patient.Id)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AllergyDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                Name = a.Name,
                Severity = a.Severity,
                Reaction = a.Reaction,
                DiagnosedDate = a.DiagnosedDate,
                Source = a.Source,
                CreatedAt = a.CreatedAt
            }).ToListAsync();

        return Ok(ApiResponse<List<AllergyDto>>.Ok(allergies));
    }

    [HttpPost("me/allergies")]
    public async Task<ActionResult<ApiResponse<AllergyDto>>> CreateAllergy([FromBody] CreateAllergyDto request)
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<AllergyDto>.Fail("Patient profile not found."));

        var allergy = new Allergy
        {
            PatientId = patient.Id,
            Name = request.Name,
            Severity = request.Severity,
            Reaction = request.Reaction,
            DiagnosedDate = request.DiagnosedDate,
            Source = "Patient",
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Allergies.Add(allergy);
        await _context.SaveChangesAsync();

        var dto = new AllergyDto
        {
            Id = allergy.Id,
            PatientId = allergy.PatientId,
            Name = allergy.Name,
            Severity = allergy.Severity,
            Reaction = allergy.Reaction,
            DiagnosedDate = allergy.DiagnosedDate,
            Source = allergy.Source,
            CreatedAt = allergy.CreatedAt
        };

        return Ok(ApiResponse<AllergyDto>.Ok(dto, "Allergy recorded successfully."));
    }

    [HttpDelete("me/allergies/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAllergy(Guid id)
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<bool>.Fail("Patient profile not found."));

        var allergy = await _context.Allergies.FirstOrDefaultAsync(a => a.Id == id && a.PatientId == patient.Id);
        if (allergy == null) return NotFound(ApiResponse<bool>.Fail("Allergy record not found."));

        _context.Allergies.Remove(allergy);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Allergy record deleted."));
    }

    [HttpGet("me/chronic-conditions")]
    public async Task<ActionResult<ApiResponse<List<ChronicConditionDto>>>> GetMyChronicConditions()
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<List<ChronicConditionDto>>.Fail("Patient profile not found."));

        var conditions = await _context.ChronicConditions
            .Where(c => c.PatientId == patient.Id)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ChronicConditionDto
            {
                Id = c.Id,
                PatientId = c.PatientId,
                Name = c.Name,
                DiagnosedDate = c.DiagnosedDate,
                Status = c.Status,
                Notes = c.Notes,
                Source = c.Source,
                CreatedAt = c.CreatedAt
            }).ToListAsync();

        return Ok(ApiResponse<List<ChronicConditionDto>>.Ok(conditions));
    }

    [HttpPost("me/chronic-conditions")]
    public async Task<ActionResult<ApiResponse<ChronicConditionDto>>> CreateChronicCondition([FromBody] CreateChronicConditionDto request)
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<ChronicConditionDto>.Fail("Patient profile not found."));

        var condition = new ChronicCondition
        {
            PatientId = patient.Id,
            Name = request.Name,
            DiagnosedDate = request.DiagnosedDate,
            Status = request.Status,
            Notes = request.Notes,
            Source = "Patient",
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ChronicConditions.Add(condition);
        await _context.SaveChangesAsync();

        var dto = new ChronicConditionDto
        {
            Id = condition.Id,
            PatientId = condition.PatientId,
            Name = condition.Name,
            DiagnosedDate = condition.DiagnosedDate,
            Status = condition.Status,
            Notes = condition.Notes,
            Source = condition.Source,
            CreatedAt = condition.CreatedAt
        };

        return Ok(ApiResponse<ChronicConditionDto>.Ok(dto, "Chronic condition recorded."));
    }

    [HttpDelete("me/chronic-conditions/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteChronicCondition(Guid id)
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<bool>.Fail("Patient profile not found."));

        var condition = await _context.ChronicConditions.FirstOrDefaultAsync(c => c.Id == id && c.PatientId == patient.Id);
        if (condition == null) return NotFound(ApiResponse<bool>.Fail("Chronic condition record not found."));

        _context.ChronicConditions.Remove(condition);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Chronic condition deleted."));
    }

    [HttpGet("me/vaccinations")]
    public async Task<ActionResult<ApiResponse<List<VaccinationDto>>>> GetMyVaccinations()
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<List<VaccinationDto>>.Fail("Patient profile not found."));

        var vax = await _context.Vaccinations
            .Where(v => v.PatientId == patient.Id)
            .OrderByDescending(v => v.DateAdministered)
            .Select(v => new VaccinationDto
            {
                Id = v.Id,
                PatientId = v.PatientId,
                Name = v.Name,
                DateAdministered = v.DateAdministered,
                Provider = v.Provider,
                DoseNumber = v.DoseNumber,
                Notes = v.Notes,
                CreatedAt = v.CreatedAt
            }).ToListAsync();

        return Ok(ApiResponse<List<VaccinationDto>>.Ok(vax));
    }

    [HttpPost("me/vaccinations")]
    public async Task<ActionResult<ApiResponse<VaccinationDto>>> CreateVaccination([FromBody] CreateVaccinationDto request)
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<VaccinationDto>.Fail("Patient profile not found."));

        var vax = new Vaccination
        {
            PatientId = patient.Id,
            Name = request.Name,
            DateAdministered = request.DateAdministered,
            Provider = request.Provider,
            DoseNumber = request.DoseNumber,
            Notes = request.Notes,
            Source = "Patient",
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Vaccinations.Add(vax);
        await _context.SaveChangesAsync();

        var dto = new VaccinationDto
        {
            Id = vax.Id,
            PatientId = vax.PatientId,
            Name = vax.Name,
            DateAdministered = vax.DateAdministered,
            Provider = vax.Provider,
            DoseNumber = vax.DoseNumber,
            Notes = vax.Notes,
            CreatedAt = vax.CreatedAt
        };

        return Ok(ApiResponse<VaccinationDto>.Ok(dto, "Vaccination recorded."));
    }

    [HttpDelete("me/vaccinations/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteVaccination(Guid id)
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<bool>.Fail("Patient profile not found."));

        var vax = await _context.Vaccinations.FirstOrDefaultAsync(v => v.Id == id && v.PatientId == patient.Id);
        if (vax == null) return NotFound(ApiResponse<bool>.Fail("Vaccination record not found."));

        _context.Vaccinations.Remove(vax);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Vaccination deleted."));
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
                BadgeColor = "blue",
                DetailsUrl = "/appointments"
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
                BadgeColor = l.Status == Domain.Enums.LabResultStatus.Normal ? "emerald" : "amber",
                DetailsUrl = "/lab-results"
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
                BadgeColor = "purple",
                DetailsUrl = "/documents"
            });
        }

        var meds = await _context.Medications.Where(m => m.PatientId == patient.Id).ToListAsync();
        foreach (var m in meds)
        {
            timeline.Add(new TimelineItemDto
            {
                Id = m.Id,
                Category = "Medication",
                Title = $"Medication: {m.Name}",
                Description = $"{m.Dosage} • {m.Frequency}",
                Date = m.StartDate,
                BadgeColor = "cyan",
                DetailsUrl = "/medications"
            });
        }

        var events = await _context.HealthEvents.Where(e => e.PatientId == patient.Id).ToListAsync();
        foreach (var e in events)
        {
            timeline.Add(new TimelineItemDto
            {
                Id = e.Id,
                Category = "HealthEvent",
                Title = e.Title,
                Description = e.Description,
                Date = e.EventDate,
                BadgeColor = "indigo",
                DetailsUrl = "/timeline"
            });
        }

        var vaxList = await _context.Vaccinations.Where(v => v.PatientId == patient.Id).ToListAsync();
        foreach (var v in vaxList)
        {
            timeline.Add(new TimelineItemDto
            {
                Id = v.Id,
                Category = "Vaccination",
                Title = $"Vaccine: {v.Name}",
                Description = $"Dose #{v.DoseNumber} by {v.Provider}",
                Date = v.DateAdministered,
                BadgeColor = "teal",
                DetailsUrl = "/health-passport"
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
                FileSizeBytes = d.FileSizeBytes,
                DocumentType = d.DocumentType,
                ExtractedText = d.ExtractedText,
                AISummary = d.AISummary,
                IsProcessed = d.IsProcessed,
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
            .Include(m => m.PrescribedByDoctor).ThenInclude(d => d!.User)
            .Where(m => m.PatientId == patient.Id)
            .OrderByDescending(m => m.StartDate)
            .Select(m => new MedicationDto
            {
                Id = m.Id,
                PatientId = m.PatientId,
                PrescribedByDoctorId = m.PrescribedByDoctorId,
                PrescribedByDoctorName = m.PrescribedByDoctor != null ? $"Dr. {m.PrescribedByDoctor.User.FirstName} {m.PrescribedByDoctor.User.LastName}" : string.Empty,
                Name = m.Name,
                Dosage = m.Dosage,
                Frequency = m.Frequency,
                Instructions = m.Instructions,
                Status = m.Status,
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
