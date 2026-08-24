using System;
using System.Linq;
using System.Threading.Tasks;
using MedAI.Application.DTOs.AI;
using MedAI.Domain.Entities;
using MedAI.Domain.Enums;
using MedAI.Infrastructure.Authentication;
using MedAI.Infrastructure.Data;
using MedAI.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedAI.Tests;

public class MedAiUnitTests
{
    private MedAiDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MedAiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new MedAiDbContext(options);
    }

    [Fact]
    public void PasswordHasher_ShouldHashAndVerifyPasswordCorrectly()
    {
        var hasher = new PasswordHasher();
        string rawPassword = "SecureMedAI123!";

        string hash = hasher.HashPassword(rawPassword);
        bool isValid = hasher.VerifyPassword(rawPassword, hash);
        bool isInvalid = hasher.VerifyPassword("WrongPassword", hash);

        Assert.True(isValid);
        Assert.False(isInvalid);
    }

    [Fact]
    public async Task AIService_ShouldDetectEmergencyAndProvideSafetyAlert()
    {
        using var context = GetInMemoryDbContext();
        var aiService = new AIService(context);
        var userId = Guid.NewGuid();

        var request = new AIChatRequestDto
        {
            Message = "I am experiencing severe chest pain and shortness of breath."
        };

        var response = await aiService.ChatAsync(userId, request);

        Assert.NotNull(response);
        Assert.Equal(SafetyLevel.EmergencyWarning, response.SafetyLevel);
        Assert.Contains("EMERGENCY ALERT", response.SafetyMessage);
    }

    [Fact]
    public async Task AIService_SymptomAnalysis_ShouldReturnDisclaimerAndRiskLevel()
    {
        using var context = GetInMemoryDbContext();
        var aiService = new AIService(context);

        var request = new SymptomAnalysisRequestDto
        {
            Symptoms = "Fever and mild persistent cough",
            Duration = "3 days",
            Age = 35
        };

        var response = await aiService.AnalyzeSymptomsAsync(request);

        Assert.NotNull(response);
        Assert.Contains("AI assists with initial symptom categorization", response.SafetyMessage);
        Assert.NotEmpty(response.PotentialCauses);
        Assert.NotEmpty(response.FollowUpQuestions);
    }

    [Fact]
    public async Task AIService_UzbekLanguage_ShouldRespondInUzbek()
    {
        using var context = GetInMemoryDbContext();
        var aiService = new AIService(context);
        var userId = Guid.NewGuid();

        var request = new AIChatRequestDto
        {
            Message = "Assalomu alaykum, boshim og'riyapti va mazam bo'mayapti"
        };

        var response = await aiService.ChatAsync(userId, request);

        Assert.NotNull(response);
        Assert.Contains("Vaalaykum assalom", response.Response);
        Assert.Equal(SafetyLevel.Safe, response.SafetyLevel);
    }

    [Fact]
    public async Task HealthPassport_Entities_ShouldPersistCorrectly()
    {
        using var context = GetInMemoryDbContext();
        var patientId = Guid.NewGuid();
        var patient = new PatientProfile
        {
            Id = patientId,
            UserId = Guid.NewGuid(),
            BloodType = "B+"
        };
        context.PatientProfiles.Add(patient);

        var allergy = new Allergy
        {
            PatientId = patientId,
            Name = "Penicillin",
            Severity = AllergySeverity.Severe,
            Reaction = "Anaphylaxis"
        };

        var condition = new ChronicCondition
        {
            PatientId = patientId,
            Name = "Type 2 Diabetes",
            Status = ChronicConditionStatus.Active
        };

        context.Allergies.Add(allergy);
        context.ChronicConditions.Add(condition);
        await context.SaveChangesAsync();

        var savedAllergies = await context.Allergies.Where(a => a.PatientId == patientId).ToListAsync();
        var savedConditions = await context.ChronicConditions.Where(c => c.PatientId == patientId).ToListAsync();

        Assert.Single(savedAllergies);
        Assert.Equal("Penicillin", savedAllergies[0].Name);
        Assert.Single(savedConditions);
        Assert.Equal("Type 2 Diabetes", savedConditions[0].Name);
    }

    [Fact]
    public async Task DataConsent_AndAccessLog_ShouldPersistSuccessfully()
    {
        using var context = GetInMemoryDbContext();
        var patientId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();

        var consent = new DataConsent
        {
            PatientId = patientId,
            GrantedToUserId = doctorUserId,
            Scope = ConsentScope.FullProfile,
            IsActive = true,
            GrantedAt = DateTime.UtcNow
        };

        var accessLog = new DataAccessLog
        {
            PatientId = patientId,
            AccessedByUserId = doctorUserId,
            AccessReason = "Clinical consultation",
            DataScope = "FullProfile",
            IpAddress = "192.168.1.10"
        };

        context.DataConsents.Add(consent);
        context.DataAccessLogs.Add(accessLog);
        await context.SaveChangesAsync();

        var activeConsent = await context.DataConsents.FirstOrDefaultAsync(c => c.PatientId == patientId && c.IsActive);
        var logs = await context.DataAccessLogs.Where(l => l.PatientId == patientId).ToListAsync();

        Assert.NotNull(activeConsent);
        Assert.Equal(doctorUserId, activeConsent.GrantedToUserId);
        Assert.Single(logs);
        Assert.Equal("192.168.1.10", logs[0].IpAddress);
    }

    [Fact]
    public async Task AIHandoffSummary_ShouldGenerateFromSession()
    {
        using var context = GetInMemoryDbContext();
        var userId = Guid.NewGuid();
        var session = new AISession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Intake Chat"
        };
        context.AISessions.Add(session);

        context.AIMessages.Add(new AIMessage
        {
            AISessionId = session.Id,
            Role = "user",
            Content = "Persistent cough for 4 days with mild fever"
        });

        var patient = new PatientProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId
        };
        context.PatientProfiles.Add(patient);
        await context.SaveChangesAsync();

        var aiService = new AIService(context);
        var summary = await aiService.GenerateHandoffSummaryAsync(session.Id, patient.Id);

        Assert.NotNull(summary);
        Assert.Equal(session.Id, summary.AISessionId);
        Assert.Contains("Persistent cough", summary.MainComplaint);
    }
}
