
namespace SurveyBasket.Api.Persistence.EntitiesConfigurations
{
    public class VoteAnswerConfigurations : IEntityTypeConfiguration<VoteAnswer>
    {
        public void Configure(EntityTypeBuilder<VoteAnswer> builder)
        {
            builder.HasIndex(x => new { x.VoteId, x.QuestionId }).IsUnique();
        }
    }
}
