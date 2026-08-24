using System;
using System.Threading.Tasks;
using MedAI.Application.DTOs.AI;
using MedAI.Application.DTOs.Clinical;

namespace MedAI.Application.Interfaces;

public interface IAIService
{
    Task<AIChatResponseDto> ChatAsync(Guid userId, AIChatRequestDto request);
    Task<SymptomAnalysisResponseDto> AnalyzeSymptomsAsync(SymptomAnalysisRequestDto request);
    Task<LabExplanationResponseDto> ExplainLabResultAsync(Guid labResultId);
    Task<DocumentAnalysisResponseDto> AnalyzeDocumentAsync(Guid documentId);
    Task<DocumentAnalysisResponseDto> AnalyzeDocumentAsync(string fileName, string extractedText);
    Task<MedicalSummaryResponseDto> GenerateMedicalSummaryAsync(Guid patientId);
    Task<DoctorBriefResponseDto> GenerateDoctorBriefAsync(Guid patientId);
    Task<AIHandoffSummaryDto> GenerateHandoffSummaryAsync(Guid sessionId, Guid patientId);
    Task<TermExplanationResponseDto> ExplainMedicalTermAsync(string term);
    Task<HealthEducationResponseDto> GenerateHealthEducationAsync(string topic, string language);
}

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Domain.Entities.User user);
    string GenerateRefreshToken();
    System.Security.Claims.ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserEmail { get; }
    string? Role { get; }
}

public interface IAuditLogService
{
    Task LogAsync(Guid? userId, string action, string entityType, string entityId, string ipAddress);
}
