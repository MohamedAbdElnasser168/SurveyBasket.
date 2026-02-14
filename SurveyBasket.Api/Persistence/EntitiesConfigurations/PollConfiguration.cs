
namespace SurveyBasket.Api.Persistence.EntitiesConfigurations
{
    // كل ده عشان استخدم الفلاونت ابي اي مع كل انتيتي لوحدها عشان الكود ميكبرش في ال DbContext
    public class PollConfiguration : IEntityTypeConfiguration<Poll>
    {
        public void Configure(EntityTypeBuilder<Poll> builder)
        {
            builder.HasIndex(p=>p.Title).IsUnique();
            builder.Property(p => p.Title).HasMaxLength(100);
            builder.Property(p => p.Summary).HasMaxLength(1500);
        }
    }
}
