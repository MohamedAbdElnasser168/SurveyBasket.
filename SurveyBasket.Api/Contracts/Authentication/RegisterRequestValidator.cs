using SurveyBasket.Api.Abstractions.Consts;
using System.Text.RegularExpressions;

namespace SurveyBasket.Api.Contracts.Authentication
{
    public class RegisterRequestValidator:AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator() 
        {
            RuleFor(x => x.Email)
                // .MustAsync() serch about it 
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
            
            
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .Matches(RegexPatterns.Password)
                .WithMessage("Password must be at least 8 characters long and contains Lowercase, Uppercase , Non-alphanumeric.");
            
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .Length(3,100);
            
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .Length(3, 100);
        
        }
    }
}
