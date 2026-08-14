namespace SurveyBasket.Api.Persistence.EntitiesConfigurations;

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        // Configure the ApplicationRole entity here

        // Default value for IsDefault property

        builder.HasData([
            new ApplicationRole
            {
                Id = DefaultRoles.AdminRoleId,
                Name = DefaultRoles.Admin,
                NormalizedName =DefaultRoles.Admin.ToUpper(),
                ConcurrencyStamp = DefaultRoles.AdminRoleConcurrencyStamp,
            },
            new ApplicationRole
            {
                Id = DefaultRoles.MemberRoleId,
                Name = DefaultRoles.Member,
                NormalizedName =DefaultRoles.Member.ToUpper(),
                ConcurrencyStamp = DefaultRoles.MemberRoleConcurrencyStamp,
                IsDefault = true
            }

            ]);

    }
}
