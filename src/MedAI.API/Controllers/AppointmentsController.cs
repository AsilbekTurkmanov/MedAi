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
[Route("api/appointments")]
[Produces("application/json")]
public class AppointmentsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public AppointmentsController(IApplicationDbContext context)
    {
        _context = context;
    }

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AppointmentDto>>> CreateAppointment([FromBody] CreateAppointmentDto request)
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.Include(p => p.User).FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return BadRequest(ApiResponse<AppointmentDto>.Fail("Only registered patients can book appointments."));

        var doctor = await _context.DoctorProfiles.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == request.DoctorId);
        if (doctor == null) return NotFound(ApiResponse<AppointmentDto>.Fail("Selected doctor not found."));

        var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == request.ClinicId) ?? await _context.Clinics.FirstOrDefaultAsync();
        if (clinic == null) return NotFound(ApiResponse<AppointmentDto>.Fail("Clinic not found."));

        TimeSpan.TryParse(request.StartTime, out var startTime);
        TimeSpan.TryParse(request.EndTime, out var endTime);
        if (endTime <= startTime) endTime = startTime.Add(TimeSpan.FromMinutes(30));

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            ClinicId = clinic.Id,
            AppointmentDate = request.AppointmentDate.Date,
            StartTime = startTime,
            EndTime = endTime,
            Status = AppointmentStatus.Pending,
            Reason = request.Reason,
            Notes = "",
            CreatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);

        // Add Notification
        _context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = patient.UserId,
            Title = "Appointment Request Submitted",
            Message = $"Your appointment request with Dr. {doctor.User.LastName} for {request.AppointmentDate:MMM dd, yyyy} is pending confirmation.",
            Type = NotificationType.AppointmentReminder,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var dto = new AppointmentDto
        {
            Id = appointment.Id,
            PatientId = patient.Id,
            PatientName = $"{patient.User.FirstName} {patient.User.LastName}",
            DoctorId = doctor.Id,
            DoctorName = $"Dr. {doctor.User.FirstName} {doctor.User.LastName}",
            DoctorSpecialization = doctor.Specialization,
            ClinicId = clinic.Id,
            ClinicName = clinic.Name,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime.ToString(@"hh\:mm"),
            EndTime = appointment.EndTime.ToString(@"hh\:mm"),
            Status = appointment.Status,
            Reason = appointment.Reason,
            Notes = appointment.Notes,
            CreatedAt = appointment.CreatedAt
        };

        return Ok(ApiResponse<AppointmentDto>.Ok(dto, "Appointment booked successfully."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AppointmentDto>>>> GetAppointments([FromQuery] AppointmentStatus? status)
    {
        var query = _context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Clinic)
            .AsQueryable();

        if (status.HasValue) query = query.Where(a => a.Status == status.Value);

        var list = await query.OrderByDescending(a => a.AppointmentDate).Select(a => new AppointmentDto
        {
            Id = a.Id,
            PatientId = a.PatientId,
            PatientName = $"{a.Patient.User.FirstName} {a.Patient.User.LastName}",
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

        return Ok(ApiResponse<List<AppointmentDto>>.Ok(list));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AppointmentDto>>> GetAppointmentById(Guid id)
    {
        var a = await _context.Appointments
            .Include(x => x.Patient).ThenInclude(p => p.User)
            .Include(x => x.Doctor).ThenInclude(d => d.User)
            .Include(x => x.Clinic)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (a == null) return NotFound(ApiResponse<AppointmentDto>.Fail("Appointment not found."));

        var dto = new AppointmentDto
        {
            Id = a.Id,
            PatientId = a.PatientId,
            PatientName = $"{a.Patient.User.FirstName} {a.Patient.User.LastName}",
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
        };

        return Ok(ApiResponse<AppointmentDto>.Ok(dto));
    }

    [HttpPost("{id}/confirm")]
    public async Task<ActionResult<ApiResponse<AppointmentDto>>> ConfirmAppointment(Guid id)
    {
        var appt = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (appt == null) return NotFound(ApiResponse<AppointmentDto>.Fail("Appointment not found."));

        appt.Status = AppointmentStatus.Confirmed;
        await _context.SaveChangesAsync();

        return await GetAppointmentById(id);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<AppointmentDto>>> CancelAppointment(Guid id)
    {
        var appt = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (appt == null) return NotFound(ApiResponse<AppointmentDto>.Fail("Appointment not found."));

        appt.Status = AppointmentStatus.Cancelled;
        await _context.SaveChangesAsync();

        return await GetAppointmentById(id);
    }
}
