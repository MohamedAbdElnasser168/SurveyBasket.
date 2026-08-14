using SurveyBasket.Api.Abstractions.Consts;

namespace SurveyBasket.Api.Contracts.Authentication;

public class ResetPasswordRequestValidator: AbstractValidator<REsetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Reset code is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .Matches(RegexPatterns.Password);
    }
}
