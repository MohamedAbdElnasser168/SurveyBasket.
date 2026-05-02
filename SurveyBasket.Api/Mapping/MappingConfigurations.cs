
using SurveyBasket.Api.Contracts.Questions;

namespace SurveyBasket.Api.Mapping
{
    public class MappingConfigurations : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<QuestionRequest, Question>()
                // Map the Answers property by projecting each string answer into an Answer entity
                .Map(dest => dest.Answers, src => src.Answers.Select(answer => new Answer { Content = answer }));
            //.Ignore(nameof(Question.Answers));

            config.NewConfig<RegisterRequest,ApplicationUser>()
                .Map(dest => dest.UserName, src => src.Email);
        }
    }
}
