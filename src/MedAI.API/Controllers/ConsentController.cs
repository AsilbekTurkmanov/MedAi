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
[Route("api/consent")]
[Produces("application/json")]
public class ConsentController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public ConsentController(IApplicationDbContext context)
    {
        _context = context;
    }

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
    }

    [HttpGet("my-consents")]
    public async Task<ActionResult<ApiResponse<List<DataConsentDto>>>> GetMyConsents()
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<List<DataConsentDto>>.Fail("Patient profile not found."));

        var consents = await _context.DataConsents
            .Include(c => c.GrantedToUser)
            .Where(c => c.PatientId == patient.Id)
            .OrderByDescending(c => c.GrantedAt)
            .Select(c => new DataConsentDto
            {
                Id = c.Id,
                PatientId = c.PatientId,
                GrantedToUserId = c.GrantedToUserId,
                GrantedToUserName = $"{c.GrantedToUser.FirstName} {c.GrantedToUser.LastName}",
                GrantedToUserRole = c.GrantedToUser.Role.ToString(),
                Scope = c.Scope,
                IsActive = c.IsActive,
                GrantedAt = c.GrantedAt,
                RevokedAt = c.RevokedAt,
                ExpiresAt = c.ExpiresAt
            }).ToListAsync();

        return Ok(ApiResponse<List<DataConsentDto>>.Ok(consents));
    }

    [HttpPost("grant")]
    public async Task<ActionResult<ApiResponse<DataConsentDto>>> GrantConsent([FromBody] GrantConsentDto request)
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<DataConsentDto>.Fail("Patient profile not found."));

        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.GrantToUserId);
        if (targetUser == null) return NotFound(ApiResponse<DataConsentDto>.Fail("User to grant consent to not found."));

        // Revoke any existing active consent to the same user
        var existing = await _context.DataConsents
            .FirstOrDefaultAsync(c => c.PatientId == patient.Id && c.GrantedToUserId == request.GrantToUserId && c.IsActive);
        if (existing != null)
        {
            existing.IsActive = false;
            existing.RevokedAt = DateTime.UtcNow;
        }

        var consent = new DataConsent
        {
            PatientId = patient.Id,
            GrantedToUserId = request.GrantToUserId,
            Scope = request.Scope,
            IsActive = true,
            GrantedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddDays(30)
        };

        _context.DataConsents.Add(consent);

        // Add log
        _context.DataAccessLogs.Add(new DataAccessLog
        {
            PatientId = patient.Id,
            AccessedByUserId = userId,
            AccessReason = $"Granted access consent to {targetUser.FirstName} {targetUser.LastName} ({request.Scope})",
            DataScope = request.Scope.ToString(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            AccessedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var dto = new DataConsentDto
        {
            Id = consent.Id,
            PatientId = consent.PatientId,
            GrantedToUserId = consent.GrantedToUserId,
            GrantedToUserName = $"{targetUser.FirstName} {targetUser.LastName}",
            GrantedToUserRole = targetUser.Role.ToString(),
            Scope = consent.Scope,
            IsActive = consent.IsActive,
            GrantedAt = consent.GrantedAt,
            ExpiresAt = consent.ExpiresAt
        };

        return Ok(ApiResponse<DataConsentDto>.Ok(dto, "Data access consent granted successfully."));
    }

    [HttpPost("revoke/{consentId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> RevokeConsent(Guid consentId)
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<bool>.Fail("Patient profile not found."));

        var consent = await _context.DataConsents
            .Include(c => c.GrantedToUser)
            .FirstOrDefaultAsync(c => c.Id == consentId && c.PatientId == patient.Id);

        if (consent == null) return NotFound(ApiResponse<bool>.Fail("Consent record not found."));

        consent.IsActive = false;
        consent.RevokedAt = DateTime.UtcNow;

        _context.DataAccessLogs.Add(new DataAccessLog
        {
            PatientId = patient.Id,
            AccessedByUserId = userId,
            AccessReason = $"Revoked access consent for {consent.GrantedToUser.FirstName} {consent.GrantedToUser.LastName}",
            DataScope = consent.Scope.ToString(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            AccessedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Consent revoked successfully."));
    }

    [HttpGet("access-log")]
    public async Task<ActionResult<ApiResponse<List<DataAccessLogDto>>>> GetAccessLog()
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<List<DataAccessLogDto>>.Fail("Patient profile not found."));

        var logs = await _context.DataAccessLogs
            .Include(l => l.AccessedByUser)
            .Where(l => l.PatientId == patient.Id)
            .OrderByDescending(l => l.AccessedAt)
            .Take(50)
            .Select(l => new DataAccessLogDto
            {
                Id = l.Id,
                PatientId = l.PatientId,
                AccessedByUserId = l.AccessedByUserId,
                AccessedByUserName = $"{l.AccessedByUser.FirstName} {l.AccessedByUser.LastName}",
                AccessReason = l.AccessReason,
                DataScope = l.DataScope,
                IpAddress = l.IpAddress,
                AccessedAt = l.AccessedAt
            }).ToListAsync();

        return Ok(ApiResponse<List<DataAccessLogDto>>.Ok(logs));
    }
}
