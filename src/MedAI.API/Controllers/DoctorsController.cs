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
    [HttpGet("{id}")]
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

    [HttpGet("me/appointments")]
    public async Task<ActionResult<ApiResponse<List<AppointmentDto>>>> GetMyDoctorAppointments()
    {
        var userId = GetCurrentUserId();
        var doctor = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
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
    [HttpGet("{id}/availability")]
    public async Task<ActionResult<ApiResponse<DoctorAvailabilityDto>>> GetDoctorAvailability(Guid id)
    {
        var doctor = await _context.DoctorProfiles.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
        if (doctor == null) return NotFound(ApiResponse<DoctorAvailabilityDto>.Fail("Doctor not found."));

        var slots = new List<string> { "09:00 AM", "09:30 AM", "10:00 AM", "10:30 AM", "11:00 AM", "02:00 PM", "02:30 PM", "03:00 PM" };
        var dto = new DoctorAvailabilityDto
        {
            DoctorId = doctor.Id,
            DoctorName = $"Dr. {doctor.User.FirstName} {doctor.User.LastName}",
            AvailableSlots = slots
        };

        return Ok(ApiResponse<DoctorAvailabilityDto>.Ok(dto));
    }
}
