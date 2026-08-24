using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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
[Route("api/qr-passport")]
[Produces("application/json")]
public class QrHealthPassportController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public QrHealthPassportController(IApplicationDbContext context)
    {
        _context = context;
    }

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<ApiResponse<QrHealthTokenDto>>> GenerateQrToken([FromBody] GenerateQrTokenDto request)
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<QrHealthTokenDto>.Fail("Patient profile not found."));

        var rawToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        var token = new QrHealthToken
        {
            PatientId = patient.Id,
            TokenHash = tokenHash,
            Scope = request.Scope,
            ExpiresAt = DateTime.UtcNow.AddMinutes(request.ExpiresInMinutes > 0 ? request.ExpiresInMinutes : 30),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.QrHealthTokens.Add(token);
        await _context.SaveChangesAsync();

        var dto = new QrHealthTokenDto
        {
            Id = token.Id,
            PatientId = token.PatientId,
            Scope = token.Scope,
            ExpiresAt = token.ExpiresAt,
            IsUsed = token.IsUsed,
            CreatedAt = token.CreatedAt
        };

        return Ok(ApiResponse<QrHealthTokenDto>.Ok(dto, "QR access token generated. Scan to view health passport."));
    }

    [AllowAnonymous]
    [HttpGet("scan/{tokenId:guid}")]
    public async Task<ActionResult<ApiResponse<HealthPassportDto>>> ScanQrToken(Guid tokenId)
    {
        var token = await _context.QrHealthTokens
            .Include(t => t.Patient).ThenInclude(p => p.User)
            .Include(t => t.Patient).ThenInclude(p => p.Medications)
            .Include(t => t.Patient).ThenInclude(p => p.LabResults)
            .Include(t => t.Patient).ThenInclude(p => p.HealthEvents)
            .Include(t => t.Patient).ThenInclude(p => p.Allergies)
            .Include(t => t.Patient).ThenInclude(p => p.ChronicConditions)
            .Include(t => t.Patient).ThenInclude(p => p.Vaccinations)
            .FirstOrDefaultAsync(t => t.Id == tokenId);

        if (token == null) return NotFound(ApiResponse<HealthPassportDto>.Fail("Invalid or expired QR token."));

        if (DateTime.UtcNow > token.ExpiresAt)
        {
            return BadRequest(ApiResponse<HealthPassportDto>.Fail("QR access token has expired. Please generate a new QR code."));
        }

        var patient = token.Patient;
        int age = DateTime.UtcNow.Year - patient.User.DateOfBirth.Year;

        // Log scan access
        _context.DataAccessLogs.Add(new DataAccessLog
        {
            PatientId = patient.Id,
            AccessedByUserId = patient.UserId,
            AccessReason = $"Scanned QR Health Passport (Scope: {token.Scope})",
            DataScope = token.Scope.ToString(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            AccessedAt = DateTime.UtcNow
        });

        token.IsUsed = true;
        await _context.SaveChangesAsync();

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
            ActiveMedications = patient.Medications.Where(m => m.Status == MedicationStatus.Active).Select(m => new MedicationSummaryDto
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

        return Ok(ApiResponse<HealthPassportDto>.Ok(dto, "QR Health Passport scanned successfully."));
    }
}
