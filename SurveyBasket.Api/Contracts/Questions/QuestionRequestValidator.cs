namespace SurveyBasket.Api.Contracts.Questions
{
    public class QuestionRequestValidator:AbstractValidator<QuestionRequest>
    {
        public QuestionRequestValidator()
        {
            RuleFor(x=>x.Content)
                .NotEmpty()
                .Length(3, 1000)
                .WithMessage("Content must be between 3 and 1000 characters");

            RuleFor(x => x.Answers)
                .NotNull()
                .WithMessage("Answers cannot be null");

            RuleFor(x => x.Answers)
                .Must(answers => answers != null && answers.Count > 1)
                .WithMessage("At least two answers are required")
                .When(x => x.Answers != null);

            // Avoid duplicate answers in the same question by ensuring that the count of distinct answers is equal to the total count of answers
            RuleFor(x => x.Answers)
                .Must(answers => answers.Distinct().Count()==answers.Count)
                .WithMessage("Answers cannot contain duplicated answers values")
                .When(x => x.Answers != null);

        }
    }
}
