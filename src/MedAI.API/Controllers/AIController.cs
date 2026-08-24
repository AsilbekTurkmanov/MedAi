using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MedAI.Application.Common;
using MedAI.Application.DTOs.AI;
using MedAI.Application.DTOs.Clinical;
using MedAI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedAI.API.Controllers;

[Authorize]
[ApiController]
[Route("api/ai")]
[Produces("application/json")]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;

    public AIController(IAIService aiService)
    {
        _aiService = aiService;
    }

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<ApiResponse<AIChatResponseDto>>> Chat([FromBody] AIChatRequestDto request)
    {
        var userId = GetCurrentUserId();
        var result = await _aiService.ChatAsync(userId, request);
        return Ok(ApiResponse<AIChatResponseDto>.Ok(result));
    }

    [HttpPost("analyze-symptoms")]
    public async Task<ActionResult<ApiResponse<SymptomAnalysisResponseDto>>> AnalyzeSymptoms([FromBody] SymptomAnalysisRequestDto request)
    {
        var result = await _aiService.AnalyzeSymptomsAsync(request);
        return Ok(ApiResponse<SymptomAnalysisResponseDto>.Ok(result));
    }

    [HttpPost("explain-lab-result/{labResultId:guid}")]
    public async Task<ActionResult<ApiResponse<LabExplanationResponseDto>>> ExplainLabResult(Guid labResultId)
    {
        var result = await _aiService.ExplainLabResultAsync(labResultId);
        return Ok(ApiResponse<LabExplanationResponseDto>.Ok(result));
    }

    [HttpPost("analyze-document/{documentId:guid}")]
    public async Task<ActionResult<ApiResponse<DocumentAnalysisResponseDto>>> AnalyzeDocument(Guid documentId)
    {
        var result = await _aiService.AnalyzeDocumentAsync(documentId);
        return Ok(ApiResponse<DocumentAnalysisResponseDto>.Ok(result));
    }

    [HttpPost("medical-summary/{patientId:guid}")]
    public async Task<ActionResult<ApiResponse<MedicalSummaryResponseDto>>> MedicalSummary(Guid patientId)
    {
        var result = await _aiService.GenerateMedicalSummaryAsync(patientId);
        return Ok(ApiResponse<MedicalSummaryResponseDto>.Ok(result));
    }

    [HttpGet("doctor-brief/{patientId:guid}")]
    public async Task<ActionResult<ApiResponse<DoctorBriefResponseDto>>> GetDoctorBrief(Guid patientId)
    {
        var result = await _aiService.GenerateDoctorBriefAsync(patientId);
        return Ok(ApiResponse<DoctorBriefResponseDto>.Ok(result));
    }

    [HttpPost("handoff-summary")]
    public async Task<ActionResult<ApiResponse<AIHandoffSummaryDto>>> GenerateHandoffSummary([FromQuery] Guid sessionId, [FromQuery] Guid patientId)
    {
        var result = await _aiService.GenerateHandoffSummaryAsync(sessionId, patientId);
        return Ok(ApiResponse<AIHandoffSummaryDto>.Ok(result));
    }

    [HttpPost("explain-term")]
    public async Task<ActionResult<ApiResponse<TermExplanationResponseDto>>> ExplainTerm([FromBody] string term)
    {
        var result = await _aiService.ExplainMedicalTermAsync(term);
        return Ok(ApiResponse<TermExplanationResponseDto>.Ok(result));
    }
}
