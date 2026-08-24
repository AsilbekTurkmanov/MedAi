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
[Route("api/family")]
[Produces("application/json")]
public class FamilyController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public FamilyController(IApplicationDbContext context)
    {
        _context = context;
    }

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<FamilyMemberDto>>>> GetFamilyMembers()
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<List<FamilyMemberDto>>.Fail("Patient profile not found."));

        var members = await _context.FamilyMembers
            .Where(f => f.OwnerPatientId == patient.Id)
            .OrderBy(f => f.Name)
            .Select(f => new FamilyMemberDto
            {
                Id = f.Id,
                OwnerPatientId = f.OwnerPatientId,
                Name = f.Name,
                Relationship = f.Relationship,
                DateOfBirth = f.DateOfBirth,
                Gender = f.Gender,
                BloodType = f.BloodType,
                LinkedPatientProfileId = f.LinkedPatientProfileId,
                Permissions = f.Permissions
            }).ToListAsync();

        return Ok(ApiResponse<List<FamilyMemberDto>>.Ok(members));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FamilyMemberDto>>> AddFamilyMember([FromBody] CreateFamilyMemberDto request)
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<FamilyMemberDto>.Fail("Patient profile not found."));

        var member = new FamilyMember
        {
            OwnerPatientId = patient.Id,
            Name = request.Name,
            Relationship = request.Relationship,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            BloodType = request.BloodType,
            Permissions = request.Permissions,
            CreatedAt = DateTime.UtcNow
        };

        _context.FamilyMembers.Add(member);
        await _context.SaveChangesAsync();

        var dto = new FamilyMemberDto
        {
            Id = member.Id,
            OwnerPatientId = member.OwnerPatientId,
            Name = member.Name,
            Relationship = member.Relationship,
            DateOfBirth = member.DateOfBirth,
            Gender = member.Gender,
            BloodType = member.BloodType,
            Permissions = member.Permissions
        };

        return Ok(ApiResponse<FamilyMemberDto>.Ok(dto, "Family member added successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveFamilyMember(Guid id)
    {
        var userId = GetCurrentUserId();
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(ApiResponse<bool>.Fail("Patient profile not found."));

        var member = await _context.FamilyMembers
            .FirstOrDefaultAsync(f => f.Id == id && f.OwnerPatientId == patient.Id);

        if (member == null) return NotFound(ApiResponse<bool>.Fail("Family member record not found."));

        _context.FamilyMembers.Remove(member);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Family member removed."));
    }
}
