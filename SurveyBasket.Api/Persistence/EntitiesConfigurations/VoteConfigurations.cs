
namespace SurveyBasket.Api.Persistence.EntitiesConfigurations
{
    public class VoteConfigurations : IEntityTypeConfiguration<Vote>
    {
        public void Configure(EntityTypeBuilder<Vote> builder)
        {
            builder.HasIndex(x=> new {x.UserId, x.PollId }).IsUnique();

        }
    }
}
