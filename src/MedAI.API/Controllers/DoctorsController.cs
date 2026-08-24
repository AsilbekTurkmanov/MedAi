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
[Route("api/doctors")]
[Produces("application/json")]
public class DoctorsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public DoctorsController(IApplicationDbContext context)
    {
        _context = context;
    }

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DoctorProfileDto>>>> GetAllDoctors()
    {
        var doctors = await _context.DoctorProfiles
            .Include(d => d.User)
            .Include(d => d.Clinic)
            .Where(d => d.IsVerified && d.User.IsActive)
            .Select(d => new DoctorProfileDto
            {
                Id = d.Id,
                UserId = d.UserId,
                FirstName = d.User.FirstName,
                LastName = d.User.LastName,
                Email = d.User.Email,
                Specialization = d.Specialization,
                LicenseNumber = d.LicenseNumber,
                ExperienceYears = d.ExperienceYears,
                Bio = d.Bio,
                ClinicId = d.ClinicId,
                ClinicName = d.Clinic.Name,
                IsVerified = d.IsVerified,
                CreatedAt = d.CreatedAt
            }).ToListAsync();

        return Ok(ApiResponse<List<DoctorProfileDto>>.Ok(doctors));
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DoctorProfileDto>>> GetDoctorById(Guid id)
    {
        var doctor = await _context.DoctorProfiles
            .Include(d => d.User)
            .Include(d => d.Clinic)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doctor == null) return NotFound(ApiResponse<DoctorProfileDto>.Fail("Doctor not found."));

        var dto = new DoctorProfileDto
        {
            Id = doctor.Id,
            UserId = doctor.UserId,
            FirstName = doctor.User.FirstName,
            LastName = doctor.User.LastName,
            Email = doctor.User.Email,
            Specialization = doctor.Specialization,
            LicenseNumber = doctor.LicenseNumber,
            ExperienceYears = doctor.ExperienceYears,
            Bio = doctor.Bio,
            ClinicId = doctor.ClinicId,
            ClinicName = doctor.Clinic.Name,
            IsVerified = doctor.IsVerified,
            CreatedAt = doctor.CreatedAt
        };

        return Ok(ApiResponse<DoctorProfileDto>.Ok(dto));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<DoctorProfileDto>>> GetMyDoctorProfile()
    {
        var userId = GetCurrentUserId();
        var doctor = await _context.DoctorProfiles
            .Include(d => d.User)
            .Include(d => d.Clinic)
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (doctor == null) return NotFound(ApiResponse<DoctorProfileDto>.Fail("Doctor profile not found."));

        var dto = new DoctorProfileDto
        {
            Id = doctor.Id,
            UserId = doctor.UserId,
            FirstName = doctor.User.FirstName,
            LastName = doctor.User.LastName,
            Email = doctor.User.Email,
            Specialization = doctor.Specialization,
            LicenseNumber = doctor.LicenseNumber,
            ExperienceYears = doctor.ExperienceYears,
            Bio = doctor.Bio,
            ClinicId = doctor.ClinicId,
            ClinicName = doctor.Clinic.Name,
            IsVerified = doctor.IsVerified,
            CreatedAt = doctor.CreatedAt
        };

        return Ok(ApiResponse<DoctorProfileDto>.Ok(dto));
    }

    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<DoctorProfileDto>>> UpdateMyDoctorProfile([FromBody] UpdateDoctorProfileDto request)
    {
        var userId = GetCurrentUserId();
        var doctor = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return NotFound(ApiResponse<DoctorProfileDto>.Fail("Doctor profile not found."));

        doctor.Specialization = request.Specialization;
        doctor.Bio = request.Bio;
        doctor.ExperienceYears = request.ExperienceYears;

        await _context.SaveChangesAsync();
        return await GetMyDoctorProfile();
    }

    [HttpGet("my-patients")]
    [HttpGet("me/patients")]
    public async Task<ActionResult<ApiResponse<List<PatientProfileDto>>>> GetMyPatients()
    {
        var userId = GetCurrentUserId();
        var doctor = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return NotFound(ApiResponse<List<PatientProfileDto>>.Fail("Doctor profile not found."));

        var patientIds = await _context.Appointments
            .Where(a => a.DoctorId == doctor.Id)
            .Select(a => a.PatientId)
            .Distinct()
            .ToListAsync();

        var patients = await _context.PatientProfiles
            .Include(p => p.User)
            .Where(p => patientIds.Contains(p.Id))
            .Select(p => new PatientProfileDto
            {
                Id = p.Id,
                UserId = p.UserId,
                FirstName = p.User.FirstName,
                LastName = p.User.LastName,
                Email = p.User.Email,
                PhoneNumber = p.User.PhoneNumber,
                DateOfBirth = p.User.DateOfBirth,
                Gender = p.User.Gender,
                BloodType = p.BloodType,
                EmergencyContactName = p.EmergencyContactName,
                EmergencyContactPhone = p.EmergencyContactPhone,
                Address = p.Address,
                CreatedAt = p.CreatedAt
            }).ToListAsync();

        return Ok(ApiResponse<List<PatientProfileDto>>.Ok(patients));
    }

    [HttpGet("my-appointments")]
    [HttpGet("me/appointments")]
    public async Task<ActionResult<ApiResponse<List<AppointmentDto>>>> GetMyDoctorAppointments()
    {
        var userId = GetCurrentUserId();
        var doctor = await _context.DoctorProfiles
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (doctor == null) return NotFound(ApiResponse<List<AppointmentDto>>.Fail("Doctor profile not found."));

        var appts = await _context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Clinic)
            .Where(a => a.DoctorId == doctor.Id)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientName = $"{a.Patient.User.FirstName} {a.Patient.User.LastName}",
                DoctorId = a.DoctorId,
                DoctorName = $"Dr. {doctor.User.FirstName} {doctor.User.LastName}",
                DoctorSpecialization = doctor.Specialization,
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

    [AllowAnonymous]
    [HttpGet("{id:guid}/availability")]
    public async Task<ActionResult<ApiResponse<DoctorAvailabilityDto>>> GetDoctorAvailability(
        Guid id, 
        [FromQuery] DateTime? date = null)
    {
        var targetDate = (date ?? DateTime.UtcNow.AddDays(1)).Date;
        var doctor = await _context.DoctorProfiles.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
        if (doctor == null) return NotFound(ApiResponse<DoctorAvailabilityDto>.Fail("Doctor not found."));

        // Check if doctor is on leave for this date
        var isOnLeave = await _context.DoctorLeaves
            .AnyAsync(l => l.DoctorId == id && l.StartDate <= targetDate && l.EndDate >= targetDate);

        var availableSlots = new List<AvailableSlotDto>();

        if (!isOnLeave)
        {
            // Map System DayOfWeek to DayOfWeekEnum
            var dayEnum = (DayOfWeekEnum)(int)targetDate.DayOfWeek;
            if (dayEnum == 0) dayEnum = DayOfWeekEnum.Sunday;

            var schedule = await _context.DoctorSchedules
                .FirstOrDefaultAsync(s => s.DoctorId == id && s.DayOfWeek == dayEnum && s.IsActive);

            if (schedule != null)
            {
                // Get existing appointments for target date
                var existingAppts = await _context.Appointments
                    .Where(a => a.DoctorId == id 
                                && a.AppointmentDate.Date == targetDate 
                                && a.Status != AppointmentStatus.Cancelled)
                    .Select(a => new { a.StartTime, a.EndTime })
                    .ToListAsync();

                var current = schedule.StartTime;
                var slotDuration = TimeSpan.FromMinutes(schedule.SlotDurationMinutes > 0 ? schedule.SlotDurationMinutes : 30);

                while (current + slotDuration <= schedule.EndTime)
                {
                    var slotStart = current;
                    var slotEnd = current + slotDuration;

                    bool isTaken = existingAppts.Any(a =>
                        (slotStart >= a.StartTime && slotStart < a.EndTime) ||
                        (slotEnd > a.StartTime && slotEnd <= a.EndTime) ||
                        (slotStart <= a.StartTime && slotEnd >= a.EndTime)
                    );

                    availableSlots.Add(new AvailableSlotDto
                    {
                        Date = targetDate,
                        StartTime = slotStart.ToString(@"hh\:mm"),
                        EndTime = slotEnd.ToString(@"hh\:mm"),
                        IsAvailable = !isTaken
                    });

                    current += slotDuration;
                }
            }
        }

        // Fallback default slots if doctor has no custom schedule set up yet
        if (availableSlots.Count == 0 && !isOnLeave)
        {
            var defaultTimes = new[] { "09:00", "09:30", "10:00", "10:30", "11:00", "14:00", "14:30", "15:00", "15:30", "16:00" };
            var existingAppts = await _context.Appointments
                .Where(a => a.DoctorId == id && a.AppointmentDate.Date == targetDate && a.Status != AppointmentStatus.Cancelled)
                .Select(a => a.StartTime.ToString(@"hh\:mm"))
                .ToListAsync();

            foreach (var timeStr in defaultTimes)
            {
                TimeSpan.TryParse(timeStr, out var ts);
                var endTimeStr = ts.Add(TimeSpan.FromMinutes(30)).ToString(@"hh\:mm");
                availableSlots.Add(new AvailableSlotDto
                {
                    Date = targetDate,
                    StartTime = timeStr,
                    EndTime = endTimeStr,
                    IsAvailable = !existingAppts.Contains(timeStr)
                });
            }
        }

        var dto = new DoctorAvailabilityDto
        {
            DoctorId = doctor.Id,
            DoctorName = $"Dr. {doctor.User.FirstName} {doctor.User.LastName}",
            AvailableSlots = availableSlots
        };

        return Ok(ApiResponse<DoctorAvailabilityDto>.Ok(dto));
    }

    // ===== DOCTOR SCHEDULE MANAGEMENT (Phase 4) =====

    [HttpGet("me/schedules")]
    public async Task<ActionResult<ApiResponse<List<DoctorScheduleDto>>>> GetMySchedules()
    {
        var userId = GetCurrentUserId();
        var doctor = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return NotFound(ApiResponse<List<DoctorScheduleDto>>.Fail("Doctor profile not found."));

        var schedules = await _context.DoctorSchedules
            .Where(s => s.DoctorId == doctor.Id)
            .OrderBy(s => s.DayOfWeek)
            .Select(s => new DoctorScheduleDto
            {
                Id = s.Id,
                DoctorId = s.DoctorId,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime.ToString(@"hh\:mm"),
                EndTime = s.EndTime.ToString(@"hh\:mm"),
                SlotDurationMinutes = s.SlotDurationMinutes,
                IsActive = s.IsActive
            }).ToListAsync();

        return Ok(ApiResponse<List<DoctorScheduleDto>>.Ok(schedules));
    }

    [HttpPost("me/schedules")]
    public async Task<ActionResult<ApiResponse<DoctorScheduleDto>>> CreateOrUpdateSchedule([FromBody] CreateDoctorScheduleDto request)
    {
        var userId = GetCurrentUserId();
        var doctor = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return NotFound(ApiResponse<DoctorScheduleDto>.Fail("Doctor profile not found."));

        if (!TimeSpan.TryParse(request.StartTime, out var startTime) || !TimeSpan.TryParse(request.EndTime, out var endTime))
        {
            return BadRequest(ApiResponse<DoctorScheduleDto>.Fail("Invalid time format. Use HH:mm format."));
        }

        var schedule = await _context.DoctorSchedules
            .FirstOrDefaultAsync(s => s.DoctorId == doctor.Id && s.DayOfWeek == request.DayOfWeek);

        if (schedule == null)
        {
            schedule = new DoctorSchedule
            {
                DoctorId = doctor.Id,
                DayOfWeek = request.DayOfWeek,
                StartTime = startTime,
                EndTime = endTime,
                SlotDurationMinutes = request.SlotDurationMinutes > 0 ? request.SlotDurationMinutes : 30,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.DoctorSchedules.Add(schedule);
        }
        else
        {
            schedule.StartTime = startTime;
            schedule.EndTime = endTime;
            schedule.SlotDurationMinutes = request.SlotDurationMinutes;
            schedule.IsActive = true;
        }

        await _context.SaveChangesAsync();

        var dto = new DoctorScheduleDto
        {
            Id = schedule.Id,
            DoctorId = schedule.DoctorId,
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime.ToString(@"hh\:mm"),
            EndTime = schedule.EndTime.ToString(@"hh\:mm"),
            SlotDurationMinutes = schedule.SlotDurationMinutes,
            IsActive = schedule.IsActive
        };

        return Ok(ApiResponse<DoctorScheduleDto>.Ok(dto, "Schedule updated successfully."));
    }

    [HttpGet("me/leaves")]
    public async Task<ActionResult<ApiResponse<List<DoctorLeaveDto>>>> GetMyLeaves()
    {
        var userId = GetCurrentUserId();
        var doctor = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return NotFound(ApiResponse<List<DoctorLeaveDto>>.Fail("Doctor profile not found."));

        var leaves = await _context.DoctorLeaves
            .Where(l => l.DoctorId == doctor.Id)
            .OrderByDescending(l => l.StartDate)
            .Select(l => new DoctorLeaveDto
            {
                Id = l.Id,
                DoctorId = l.DoctorId,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Reason = l.Reason
            }).ToListAsync();

        return Ok(ApiResponse<List<DoctorLeaveDto>>.Ok(leaves));
    }

    [HttpPost("me/leaves")]
    public async Task<ActionResult<ApiResponse<DoctorLeaveDto>>> AddLeave([FromBody] CreateDoctorLeaveDto request)
    {
        var userId = GetCurrentUserId();
        var doctor = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return NotFound(ApiResponse<DoctorLeaveDto>.Fail("Doctor profile not found."));

        var leave = new DoctorLeave
        {
            DoctorId = doctor.Id,
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            Reason = request.Reason,
            CreatedAt = DateTime.UtcNow
        };

        _context.DoctorLeaves.Add(leave);
        await _context.SaveChangesAsync();

        var dto = new DoctorLeaveDto
        {
            Id = leave.Id,
            DoctorId = leave.DoctorId,
            StartDate = leave.StartDate,
            EndDate = leave.EndDate,
            Reason = leave.Reason
        };

        return Ok(ApiResponse<DoctorLeaveDto>.Ok(dto, "Leave scheduled successfully."));
    }

    [HttpDelete("me/leaves/{leaveId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteLeave(Guid leaveId)
    {
        var userId = GetCurrentUserId();
        var doctor = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return NotFound(ApiResponse<bool>.Fail("Doctor profile not found."));

        var leave = await _context.DoctorLeaves.FirstOrDefaultAsync(l => l.Id == leaveId && l.DoctorId == doctor.Id);
        if (leave == null) return NotFound(ApiResponse<bool>.Fail("Leave record not found."));

        _context.DoctorLeaves.Remove(leave);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Leave cancelled."));
    }
}
