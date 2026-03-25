
namespace SurveyBasket.Api.Persistence.EntitiesConfigurations
{
    public class QuestionConfigurations : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.HasIndex(x => new { x.PollId, x.Content }).IsUnique();
            builder.Property(x => x.Content).HasMaxLength(1000);
        }
    }
}
