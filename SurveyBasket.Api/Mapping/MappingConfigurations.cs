
namespace SurveyBasket.Api.Mapping
{
    public class MappingConfigurations : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
                             //src  //dest         
            // config.NewConfig<Poll, PollResponse>()
            //.Map(dest => dest.Notes, src => src.Description);

        }
    }
}
