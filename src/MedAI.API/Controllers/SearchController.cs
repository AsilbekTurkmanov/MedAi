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
[Route("api/search")]
[Produces("application/json")]
public class SearchController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public SearchController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SearchResultDto>>>> GlobalSearch([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
        {
            return Ok(ApiResponse<List<SearchResultDto>>.Ok(new List<SearchResultDto>()));
        }

        var term = q.Trim().ToLower();
        var results = new List<SearchResultDto>();

        // Search Doctors
        var doctors = await _context.DoctorProfiles
            .Include(d => d.User)
            .Where(d => d.User.FirstName.ToLower().Contains(term) ||
                        d.User.LastName.ToLower().Contains(term) ||
                        d.Specialization.ToLower().Contains(term))
            .Take(5)
            .ToListAsync();

        foreach (var d in doctors)
        {
            results.Add(new SearchResultDto
            {
                Type = "Doctor",
                Id = d.Id,
                Title = $"Dr. {d.User.FirstName} {d.User.LastName}",
                Description = $"{d.Specialization} • {d.ExperienceYears} yrs exp"
            });
        }

        // Search Clinics
        var clinics = await _context.Clinics
            .Where(c => c.Name.ToLower().Contains(term) || c.Address.ToLower().Contains(term))
            .Take(5)
            .ToListAsync();

        foreach (var c in clinics)
        {
            results.Add(new SearchResultDto
            {
                Type = "Clinic",
                Id = c.Id,
                Title = c.Name,
                Description = c.Address
            });
        }

        // Search Medical Articles
        var articles = await _context.MedicalArticles
            .Where(a => a.IsPublished && (a.Title.ToLower().Contains(term) || a.Category.ToLower().Contains(term)))
            .Take(5)
            .ToListAsync();

        foreach (var a in articles)
        {
            results.Add(new SearchResultDto
            {
                Type = "Article",
                Id = a.Id,
                Title = a.Title,
                Description = $"Category: {a.Category}"
            });
        }

        return Ok(ApiResponse<List<SearchResultDto>>.Ok(results));
    }
}
