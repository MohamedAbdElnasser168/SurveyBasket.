namespace SurveyBasket.Api.Contracts.Polls
{
    public class PollRequestValidator:AbstractValidator<PollRequest>
    {
        public PollRequestValidator()
        {
            // Title
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Enter a {PropertyName}")
                .Length(3, 100)
                .WithMessage("Title must be between 3 and 100 characters");
           

            // Time

            RuleFor(x => x.StartsAt)
                .NotEmpty()
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));

            RuleFor(x => x.EndsAt)
                .NotEmpty();

            RuleFor(x => x)
                .Must(HasValidDates)
                //.WithName(nameof(PollRequest.EndsAt))
                .WithName("Ends At Date")
                .WithMessage("{PropertyName} must be greater than or equal to StartsAt");
            // 
            // x=>x دي معناها اني بعمل فاليديت علي الموديل كامل عشان انا محتاج اتنين بروبيرتز منه



        }

        private bool HasValidDates(PollRequest poll)
        {
            return poll.EndsAt >= poll.StartsAt;
        }
    }
}
