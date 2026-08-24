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

        var appointmentDate = request.AppointmentDate.Date;

        // Check if doctor is on leave on this date
        var isOnLeave = await _context.DoctorLeaves.AnyAsync(l => l.DoctorId == doctor.Id && l.StartDate <= appointmentDate && l.EndDate >= appointmentDate);
        if (isOnLeave)
        {
            return BadRequest(ApiResponse<AppointmentDto>.Fail("The selected doctor is on leave on this date. Please choose another date."));
        }

        // Double-booking prevention: Check if doctor already has an active overlapping appointment
        var isDoubleBooked = await _context.Appointments.AnyAsync(a =>
            a.DoctorId == doctor.Id &&
            a.AppointmentDate.Date == appointmentDate &&
            a.Status != AppointmentStatus.Cancelled &&
            ((startTime >= a.StartTime && startTime < a.EndTime) ||
             (endTime > a.StartTime && endTime <= a.EndTime) ||
             (startTime <= a.StartTime && endTime >= a.EndTime))
        );

        if (isDoubleBooked)
        {
            return BadRequest(ApiResponse<AppointmentDto>.Fail("The selected time slot is already booked for this doctor. Please choose a different time slot."));
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            ClinicId = clinic.Id,
            AppointmentDate = appointmentDate,
            StartTime = startTime,
            EndTime = endTime,
            Status = AppointmentStatus.Pending,
            Reason = request.Reason,
            Notes = "",
            CreatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);

        // Add Notification for Patient
        _context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = patient.UserId,
            Title = "Appointment Request Submitted",
            Message = $"Your appointment request with Dr. {doctor.User.LastName} for {request.AppointmentDate:MMM dd, yyyy} at {request.StartTime} is pending confirmation.",
            Type = NotificationType.AppointmentReminder,
            Priority = NotificationPriority.Normal,
            ActionUrl = "/appointments",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        // Add Notification for Doctor
        _context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = doctor.UserId,
            Title = "New Appointment Request",
            Message = $"New appointment request from {patient.User.FirstName} {patient.User.LastName} for {request.AppointmentDate:MMM dd, yyyy} at {request.StartTime}.",
            Type = NotificationType.AppointmentReminder,
            Priority = NotificationPriority.High,
            ActionUrl = "/doctors/dashboard",
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

    [HttpGet("{id:guid}")]
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

    [HttpPost("{id:guid}/confirm")]
    public async Task<ActionResult<ApiResponse<AppointmentDto>>> ConfirmAppointment(Guid id)
    {
        var appt = await _context.Appointments
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (appt == null) return NotFound(ApiResponse<AppointmentDto>.Fail("Appointment not found."));

        appt.Status = AppointmentStatus.Confirmed;

        _context.Notifications.Add(new Notification
        {
            UserId = appt.Patient.UserId,
            Title = "Appointment Confirmed",
            Message = $"Your appointment on {appt.AppointmentDate:MMM dd, yyyy} has been confirmed.",
            Type = NotificationType.AppointmentConfirmed,
            Priority = NotificationPriority.High,
            ActionUrl = "/appointments",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return await GetAppointmentById(id);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<AppointmentDto>>> CancelAppointment(Guid id)
    {
        var appt = await _context.Appointments
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (appt == null) return NotFound(ApiResponse<AppointmentDto>.Fail("Appointment not found."));

        appt.Status = AppointmentStatus.Cancelled;

        _context.Notifications.Add(new Notification
        {
            UserId = appt.Patient.UserId,
            Title = "Appointment Cancelled",
            Message = $"Your appointment scheduled for {appt.AppointmentDate:MMM dd, yyyy} was cancelled.",
            Type = NotificationType.AppointmentCancelled,
            Priority = NotificationPriority.Normal,
            ActionUrl = "/appointments",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return await GetAppointmentById(id);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ApiResponse<AppointmentDto>>> CompleteAppointment(Guid id)
    {
        var appt = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (appt == null) return NotFound(ApiResponse<AppointmentDto>.Fail("Appointment not found."));

        appt.Status = AppointmentStatus.Completed;
        await _context.SaveChangesAsync();

        return await GetAppointmentById(id);
    }

    [HttpPost("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<AppointmentDto>>> UpdateStatus(Guid id, [FromBody] UpdateAppointmentDto request)
    {
        var appt = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (appt == null) return NotFound(ApiResponse<AppointmentDto>.Fail("Appointment not found."));

        appt.Status = request.Status;
        if (!string.IsNullOrEmpty(request.Notes)) appt.Notes = request.Notes;

        await _context.SaveChangesAsync();

        return await GetAppointmentById(id);
    }
}
