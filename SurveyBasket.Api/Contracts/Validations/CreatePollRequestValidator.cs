
namespace SurveyBasket.Api.Contracts.Validations
{
    public class CreatePollRequestValidator:AbstractValidator<CreatePollRequest>
    {
        public CreatePollRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Enter a {PropertyName}")
                .Length(3, 100)
                .WithMessage("Title must be between 3 and 100 characters");
            //.MinimumLength(3)
            //.MaximumLength(100)
        }
    }
}
