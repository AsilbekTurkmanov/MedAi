using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MedAI.Application.Common;
using MedAI.Application.DTOs.Auth;
using MedAI.Application.Interfaces;
using MedAI.Domain.Entities;
using MedAI.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedAI.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwtGenerator;

    public AuthController(IApplicationDbContext context, IPasswordHasher hasher, IJwtTokenGenerator jwtGenerator)
    {
        _context = context;
        _hasher = hasher;
        _jwtGenerator = jwtGenerator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterRequestDto request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Email is already registered."));
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = _hasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Role = request.Role,
            PreferredLanguage = request.PreferredLanguage,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        Guid? patientId = null;
        Guid? doctorId = null;

        if (request.Role == UserRole.Patient)
        {
            var patient = new PatientProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                BloodType = "Unknown",
                CreatedAt = DateTime.UtcNow
            };
            _context.PatientProfiles.Add(patient);
            patientId = patient.Id;
        }
        else if (request.Role == UserRole.Doctor)
        {
            var clinic = await _context.Clinics.FirstOrDefaultAsync();
            var doctor = new DoctorProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Specialization = request.Specialization ?? "General Practitioner",
                LicenseNumber = request.LicenseNumber ?? "LIC-PENDING",
                ExperienceYears = request.ExperienceYears ?? 1,
                ClinicId = clinic?.Id ?? Guid.NewGuid(),
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.DoctorProfiles.Add(doctor);
            doctorId = doctor.Id;
        }

        user.RefreshToken = _jwtGenerator.GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync();

        var token = _jwtGenerator.GenerateAccessToken(user);

        var response = new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            AccessToken = token,
            RefreshToken = user.RefreshToken,
            AccessTokenExpiresAt = DateTime.UtcNow.AddHours(8),
            PatientId = patientId,
            DoctorId = doctorId
        };

        return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Registration successful."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.PatientProfile)
            .Include(u => u.DoctorProfile)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());

        if (user == null || !_hasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Invalid credentials."));
        }

        if (!user.IsActive)
        {
            return StatusCode(403, ApiResponse<AuthResponseDto>.Fail("User account is inactive."));
        }

        user.RefreshToken = _jwtGenerator.GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        var token = _jwtGenerator.GenerateAccessToken(user);

        var response = new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            AccessToken = token,
            RefreshToken = user.RefreshToken,
            AccessTokenExpiresAt = DateTime.UtcNow.AddHours(8),
            PatientId = user.PatientProfile?.Id,
            DoctorId = user.DoctorProfile?.Id
        };

        return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Login successful."));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var principal = _jwtGenerator.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Invalid access token or refresh token."));
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Invalid token payload."));
        }

        var user = await _context.Users
            .Include(u => u.PatientProfile)
            .Include(u => u.DoctorProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Invalid or expired refresh token."));
        }

        user.RefreshToken = _jwtGenerator.GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        var newToken = _jwtGenerator.GenerateAccessToken(user);

        var response = new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            AccessToken = newToken,
            RefreshToken = user.RefreshToken,
            AccessTokenExpiresAt = DateTime.UtcNow.AddHours(8),
            PatientId = user.PatientProfile?.Id,
            DoctorId = user.DoctorProfile?.Id
        };

        return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Token refreshed successfully."));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<string>>> Logout()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdStr, out var userId))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                user.RefreshToken = null;
                await _context.SaveChangesAsync();
            }
        }
        return Ok(ApiResponse<string>.Ok("Logged out successfully."));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserMeDto>>> GetMe()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized(ApiResponse<UserMeDto>.Fail("Unauthorized"));
        }

        var user = await _context.Users
            .Include(u => u.PatientProfile)
            .Include(u => u.DoctorProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound(ApiResponse<UserMeDto>.Fail("User not found"));

        var dto = new UserMeDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Role = user.Role,
            PreferredLanguage = user.PreferredLanguage,
            PatientId = user.PatientProfile?.Id,
            DoctorId = user.DoctorProfile?.Id
        };

        return Ok(ApiResponse<UserMeDto>.Ok(dto));
    }
}
