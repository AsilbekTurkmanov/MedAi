using System;
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
}
