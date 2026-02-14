
namespace SurveyBasket.Api.Persistence.EntitiesConfigurations
{
    // كل ده عشان استخدم الفلاونت ابي اي مع كل انتيتي لوحدها عشان الكود ميكبرش في ال DbContext
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {

            builder
                .OwnsMany(x => x.RefreshTokens)
                .ToTable("RefreshTokens")
                .WithOwner()
                .HasForeignKey("UserId");


            builder.Property(u => u.FirstName)
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .HasMaxLength(100);
                
        }
    }
}
