namespace SurveyBasket.Api.Contracts.Votes
{
    public class VoteRequestValidator:AbstractValidator<VoteRequest>
    {
        public VoteRequestValidator()
        {
            RuleFor(x => x.Answers)
                .NotEmpty()
                .WithMessage("Must Contain atleast 1 answer");

            // to validate each answer in the list of answers, we can use RuleForEach to active VoteAnswerRequestValidator 
            RuleForEach(x => x.Answers)
                .SetInheritanceValidator(v => v.Add(new VoteAnswerRequestValidator()));
        }
    }
}
