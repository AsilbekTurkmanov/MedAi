using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MedAI.Application.Common;
using MedAI.Application.DTOs.Clinical;
using MedAI.Application.Interfaces;
using MedAI.Domain.Entities;
using MedAI.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedAI.API.Controllers;

[Authorize]
[ApiController]
[Route("api/lab-results")]
[Produces("application/json")]
public class LabResultsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public LabResultsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<LabResultDto>>>> GetLabResults([FromQuery] Guid? patientId)
    {
        var query = _context.LabResults
            .Include(l => l.Patient).ThenInclude(p => p.User)
            .Include(l => l.Doctor).ThenInclude(d => d.User)
            .AsQueryable();

        if (patientId.HasValue) query = query.Where(l => l.PatientId == patientId.Value);

        var list = await query.OrderByDescending(l => l.TestDate).Select(l => new LabResultDto
        {
            Id = l.Id,
            PatientId = l.PatientId,
            PatientName = $"{l.Patient.User.FirstName} {l.Patient.User.LastName}",
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

        return Ok(ApiResponse<List<LabResultDto>>.Ok(list));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LabResultDto>>> CreateLabResult([FromBody] CreateLabResultDto request)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdStr, out var userId);
        var doctor = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId)
                     ?? await _context.DoctorProfiles.FirstOrDefaultAsync();

        if (doctor == null) return BadRequest(ApiResponse<LabResultDto>.Fail("Doctor authorization required."));

        var lab = new LabResult
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            DoctorId = doctor.Id,
            TestName = request.TestName,
            Value = request.Value,
            Unit = request.Unit,
            ReferenceRange = request.ReferenceRange,
            Status = request.Status,
            TestDate = request.TestDate,
            Notes = request.Notes
        };

        _context.LabResults.Add(lab);
        await _context.SaveChangesAsync();

        var patient = await _context.PatientProfiles.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == request.PatientId);

        var dto = new LabResultDto
        {
            Id = lab.Id,
            PatientId = lab.PatientId,
            PatientName = patient != null ? $"{patient.User.FirstName} {patient.User.LastName}" : "Patient",
            DoctorId = doctor.Id,
            DoctorName = $"Dr. {doctor.User?.FirstName} {doctor.User?.LastName}",
            TestName = lab.TestName,
            Value = lab.Value,
            Unit = lab.Unit,
            ReferenceRange = lab.ReferenceRange,
            Status = lab.Status,
            TestDate = lab.TestDate,
            Notes = lab.Notes
        };

        return Ok(ApiResponse<LabResultDto>.Ok(dto, "Lab result added successfully."));
    }
}

[Authorize]
[ApiController]
[Route("api/medications")]
[Produces("application/json")]
public class MedicationsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public MedicationsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<MedicationDto>>>> GetMedications([FromQuery] Guid? patientId)
    {
        var query = _context.Medications.AsQueryable();
        if (patientId.HasValue) query = query.Where(m => m.PatientId == patientId.Value);

        var meds = await query.OrderByDescending(m => m.StartDate).Select(m => new MedicationDto
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

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MedicationDto>>> CreateMedication([FromBody] CreateMedicationDto request)
    {
        var med = new Medication
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            Name = request.Name,
            Dosage = request.Dosage,
            Frequency = request.Frequency,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Medications.Add(med);
        await _context.SaveChangesAsync();

        var dto = new MedicationDto
        {
            Id = med.Id,
            PatientId = med.PatientId,
            Name = med.Name,
            Dosage = med.Dosage,
            Frequency = med.Frequency,
            StartDate = med.StartDate,
            EndDate = med.EndDate,
            Notes = med.Notes,
            CreatedAt = med.CreatedAt
        };

        return Ok(ApiResponse<MedicationDto>.Ok(dto, "Medication recorded successfully."));
    }
}

[Authorize]
[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public NotificationsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetNotifications()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdStr, out var userId);

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToListAsync();

        return Ok(ApiResponse<List<NotificationDto>>.Ok(notifications));
    }

    [HttpPost("{id}/read")]
    public async Task<ActionResult<ApiResponse<string>>> MarkAsRead(Guid id)
    {
        var notif = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);
        if (notif != null)
        {
            notif.IsRead = true;
            await _context.SaveChangesAsync();
        }
        return Ok(ApiResponse<string>.Ok("Notification marked as read."));
    }
}

[Authorize(Roles = "SuperAdmin,ClinicAdmin")]
[ApiController]
[Route("api/admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public AdminController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<ApiResponse<AdminDashboardStatsDto>>> GetStats()
    {
        var stats = new AdminDashboardStatsDto
        {
            TotalUsers = await _context.Users.CountAsync(),
            TotalPatients = await _context.PatientProfiles.CountAsync(),
            TotalDoctors = await _context.DoctorProfiles.CountAsync(),
            TotalClinics = await _context.Clinics.CountAsync(),
            TotalAppointments = await _context.Appointments.CountAsync(),
            TotalAiSessions = await _context.AISessions.CountAsync(),
            PendingAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending),
            CompletedAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed)
        };

        return Ok(ApiResponse<AdminDashboardStatsDto>.Ok(stats));
    }

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<List<UserManageDto>>>> GetUsers()
    {
        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserManageDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToListAsync();

        return Ok(ApiResponse<List<UserManageDto>>.Ok(users));
    }

    [HttpGet("audit-logs")]
    public async Task<ActionResult<ApiResponse<List<AuditLogDto>>>> GetAuditLogs()
    {
        var logs = await _context.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Take(100)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserEmail = a.User != null ? a.User.Email : "System",
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                IpAddress = a.IpAddress,
                CreatedAt = a.CreatedAt
            }).ToListAsync();

        return Ok(ApiResponse<List<AuditLogDto>>.Ok(logs));
    }
}
