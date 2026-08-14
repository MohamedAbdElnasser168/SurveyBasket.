

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


            // Default Data ( Seeding )

            var passwordHasher = new PasswordHasher<ApplicationUser>();

            builder.HasData(new ApplicationUser 
            {
                Id = DefaultUsers.AdminId,
                FirstName = "SurveyBasket",
                LastName = "Admin",
                UserName = DefaultUsers.AdminEmail,
                NormalizedUserName = DefaultUsers.AdminEmail.ToUpper(),
                Email = DefaultUsers.AdminEmail,
                NormalizedEmail = DefaultUsers.AdminEmail.ToUpper(),
                SecurityStamp = DefaultUsers.AdminSecurityStamp,
                ConcurrencyStamp = DefaultUsers.AdminConcurrencyStamp,
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEEYKHTCzKJC5Y4WXB/aalWsUrl5EPNzURlCk78PfvJ0nzGRgQqyquUj8qT8jDCYcnQ=="
            });
                
        }
    }
}
