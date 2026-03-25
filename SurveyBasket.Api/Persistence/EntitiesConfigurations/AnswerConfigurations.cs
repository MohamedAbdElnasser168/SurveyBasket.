namespace SurveyBasket.Api.Persistence.EntitiesConfigurations
{
    public class AnswerConfigurations:IEntityTypeConfiguration<Answer>
    {
        public void Configure(EntityTypeBuilder<Answer> builder)
        {
            builder.HasIndex(x => new { x.QuestionId, x.Content }).IsUnique();
            builder.Property(x => x.Content).HasMaxLength(1000);

        }
    }
}
