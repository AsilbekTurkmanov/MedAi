using FluentValidation;
using MedAI.Application.DTOs.AI;
using MedAI.Application.DTOs.Auth;
using MedAI.Application.DTOs.Clinical;

namespace MedAI.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class AIChatRequestValidator : AbstractValidator<AIChatRequestDto>
{
    public AIChatRequestValidator()
    {
        RuleFor(x => x.Message).NotEmpty().WithMessage("Message cannot be empty.");
    }
}

public class SymptomAnalysisRequestValidator : AbstractValidator<SymptomAnalysisRequestDto>
{
    public SymptomAnalysisRequestValidator()
    {
        RuleFor(x => x.Symptoms).NotEmpty().WithMessage("Symptoms description is required.");
    }
}

public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentDto>
{
    public CreateAppointmentValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
    }
}
