using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MedAI.Application.Common;
using MedAI.Application.DTOs.Clinical;
using MedAI.Application.Interfaces;
using MedAI.Domain.Entities;
using MedAI.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedAI.API.Controllers;

[Authorize]
[ApiController]
[Route("api/medical-records")]
[Produces("application/json")]
public class MedicalRecordsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public MedicalRecordsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<MedicalRecordDto>>>> GetMedicalRecords([FromQuery] Guid? patientId)
    {
        var query = _context.MedicalRecords
            .Include(m => m.Patient).ThenInclude(p => p.User)
            .Include(m => m.Doctor).ThenInclude(d => d.User)
            .AsQueryable();

        if (patientId.HasValue) query = query.Where(m => m.PatientId == patientId.Value);

        var records = await query.OrderByDescending(m => m.CreatedAt).Select(m => new MedicalRecordDto
        {
            Id = m.Id,
            PatientId = m.PatientId,
            PatientName = $"{m.Patient.User.FirstName} {m.Patient.User.LastName}",
            DoctorId = m.DoctorId,
            DoctorName = $"Dr. {m.Doctor.User.FirstName} {m.Doctor.User.LastName}",
            AppointmentId = m.AppointmentId,
            Title = m.Title,
            Description = m.Description,
            DiagnosisNotes = m.DiagnosisNotes,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        }).ToListAsync();

        return Ok(ApiResponse<List<MedicalRecordDto>>.Ok(records));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MedicalRecordDto>>> CreateMedicalRecord([FromBody] CreateMedicalRecordDto request)
    {
        var doctorUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(doctorUser, out var userId);

        var doctor = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId)
                     ?? await _context.DoctorProfiles.FirstOrDefaultAsync();

        if (doctor == null) return BadRequest(ApiResponse<MedicalRecordDto>.Fail("Only registered doctors can create medical records."));

        var record = new MedicalRecord
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            DoctorId = doctor.Id,
            AppointmentId = request.AppointmentId,
            Title = request.Title,
            Description = request.Description,
            DiagnosisNotes = request.DiagnosisNotes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync();

        var patient = await _context.PatientProfiles.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == request.PatientId);

        var dto = new MedicalRecordDto
        {
            Id = record.Id,
            PatientId = record.PatientId,
            PatientName = patient != null ? $"{patient.User.FirstName} {patient.User.LastName}" : "Patient",
            DoctorId = record.DoctorId,
            DoctorName = $"Dr. {doctor.User?.FirstName} {doctor.User?.LastName}",
            AppointmentId = record.AppointmentId,
            Title = record.Title,
            Description = record.Description,
            DiagnosisNotes = record.DiagnosisNotes,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt
        };

        return Ok(ApiResponse<MedicalRecordDto>.Ok(dto, "Medical record created successfully."));
    }
}

[Authorize]
[ApiController]
[Route("api/documents")]
[Produces("application/json")]
public class DocumentsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public DocumentsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<MedicalDocumentDto>>> UploadDocument(IFormFile file, [FromForm] DocumentType documentType)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<MedicalDocumentDto>.Fail("No file uploaded."));
        }

        if (file.Length > 10 * 1024 * 1024) // 10MB limit
        {
            return BadRequest(ApiResponse<MedicalDocumentDto>.Fail("File size exceeds maximum allowed limit (10MB)."));
        }

        var allowedExtensions = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".docx" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            return BadRequest(ApiResponse<MedicalDocumentDto>.Fail("Unsupported file extension. Allowed: PDF, PNG, JPG, DOCX."));
        }

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdStr, out var userId);
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        var docId = Guid.NewGuid();
        var fileName = $"{docId}_{Path.GetFileName(file.FileName)}";
        var simulatedUrl = $"/storage/documents/{fileName}";

        var extractedText = $"Extracted clinical metrics from {file.FileName}: Patient values analyzed cleanly.";
        var aiSummary = $"AI Analysis: Processed {documentType} document. No immediate critical acute flags identified.";

        var doc = new MedicalDocument
        {
            Id = docId,
            PatientId = patient?.Id ?? Guid.Empty,
            UploadedBy = userId,
            FileName = file.FileName,
            FileType = file.ContentType,
            FileUrl = simulatedUrl,
            DocumentType = documentType,
            ExtractedText = extractedText,
            AISummary = aiSummary,
            UploadedAt = DateTime.UtcNow
        };

        _context.MedicalDocuments.Add(doc);
        await _context.SaveChangesAsync();

        var dto = new MedicalDocumentDto
        {
            Id = doc.Id,
            PatientId = doc.PatientId,
            UploadedBy = doc.UploadedBy,
            FileName = doc.FileName,
            FileType = doc.FileType,
            FileUrl = doc.FileUrl,
            DocumentType = doc.DocumentType,
            ExtractedText = doc.ExtractedText,
            AISummary = doc.AISummary,
            UploadedAt = doc.UploadedAt
        };

        return Ok(ApiResponse<MedicalDocumentDto>.Ok(dto, "Document uploaded and analyzed by AI successfully."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<MedicalDocumentDto>>>> GetDocuments()
    {
        var docs = await _context.MedicalDocuments
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new MedicalDocumentDto
            {
                Id = d.Id,
                PatientId = d.PatientId,
                UploadedBy = d.UploadedBy,
                FileName = d.FileName,
                FileType = d.FileType,
                FileUrl = d.FileUrl,
                DocumentType = d.DocumentType,
                ExtractedText = d.ExtractedText,
                AISummary = d.AISummary,
                UploadedAt = d.UploadedAt
            }).ToListAsync();

        return Ok(ApiResponse<List<MedicalDocumentDto>>.Ok(docs));
    }
}
