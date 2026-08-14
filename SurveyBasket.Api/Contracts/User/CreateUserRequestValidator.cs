namespace SurveyBasket.Api.Contracts.User;

public class CreateUserRequestValidator:AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Length(3, 100).WithMessage("Min is 3 and Max is 100");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .Length(3, 100).WithMessage("Min is 3 and Max is 100");

        RuleFor(x => x.Email)
            .EmailAddress()
            .NotEmpty();

        RuleFor(x => x.Password)
            .NotEmpty()
            .Matches(RegexPatterns.Password)
            .WithMessage("Password should match the pattern");

        RuleFor(x => x.Roles)
            .NotEmpty()
            .NotNull();
        
        RuleFor(x => x.Roles)
            .Must(x => x.Distinct().Count() == x.Count)
            .WithMessage("Can't add dub role for the same user")
            .When(x=>x.Roles != null);

           
    }
}
