using System;
using System.Collections.Generic;
using MedAI.Domain.Enums;

namespace MedAI.Application.DTOs.AI;

public class AIChatRequestDto
{
    public Guid? SessionId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class AIChatResponseDto
{
    public Guid SessionId { get; set; }
    public string Response { get; set; } = string.Empty;
    public SafetyLevel SafetyLevel { get; set; } = SafetyLevel.Safe;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string SafetyMessage { get; set; } = "MedAI is an AI health assistant. This is for informational purposes only and does not constitute a formal diagnosis or medical prescription.";
}

public class SymptomAnalysisRequestDto
{
    public string Symptoms { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public int Age { get; set; } = 30;
    public string RelevantContext { get; set; } = string.Empty;
}

public class SymptomAnalysisResponseDto
{
    public string Summary { get; set; } = string.Empty;
    public List<string> FollowUpQuestions { get; set; } = new();
    public string RiskLevel { get; set; } = "Low"; // Low, Moderate, High, Emergency
    public string RecommendedNextStep { get; set; } = string.Empty;
    public string SafetyMessage { get; set; } = "AI does not provide a final diagnosis. Consult a certified medical professional.";
    public List<string> PotentialCauses { get; set; } = new();
}

public class LabExplanationResponseDto
{
    public Guid LabResultId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ReferenceRange { get; set; } = string.Empty;
    public string SimpleExplanation { get; set; } = string.Empty;
    public string TrendAnalysis { get; set; } = string.Empty;
    public List<string> QuestionsForDoctor { get; set; } = new();
    public string SafetyDisclaimer { get; set; } = "Lab explanations are for educational clarity. Discuss all results with your healthcare provider.";
}

public class DocumentAnalysisResponseDto
{
    public Guid DocumentId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string ExtractedText { get; set; } = string.Empty;
    public string AISummary { get; set; } = string.Empty;
    public List<string> KeyFindings { get; set; } = new();
    public List<string> ActionableRecommendations { get; set; } = new();
}

public class MedicalSummaryResponseDto
{
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string CurrentConcern { get; set; } = string.Empty;
    public List<string> RelevantHistory { get; set; } = new();
    public List<string> CurrentMedications { get; set; } = new();
    public List<string> Allergies { get; set; } = new();
    public List<string> RecentLabResults { get; set; } = new();
    public List<string> RecentTimelineEvents { get; set; } = new();
    public List<string> QuestionsToConsider { get; set; } = new();
    public bool IsAiGenerated { get; set; } = true;
}

public class DoctorBriefResponseDto
{
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string BloodType { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Overview { get; set; } = string.Empty;
    public List<string> ActiveMedications { get; set; } = new();
    public List<string> CriticalLabAlerts { get; set; } = new();
    public List<string> RecentAppointments { get; set; } = new();
    public List<string> RecommendedClinicalFocus { get; set; } = new();
}

public class TermExplanationResponseDto
{
    public string Term { get; set; } = string.Empty;
    public string PlainDefinition { get; set; } = string.Empty;
    public string ClinicalContext { get; set; } = string.Empty;
    public List<string> CommonExamples { get; set; } = new();
}

public class HealthEducationResponseDto
{
    public string Topic { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> KeyTakeaways { get; set; } = new();
    public List<string> LifestyleTips { get; set; } = new();
}
