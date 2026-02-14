
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SurveyBasket.Api.Persistence.EntitiesConfigurations;

namespace SurveyBasket.Api.Persistence
{
    // Prime Constractors feature used here (C# 9.0 and later)
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):IdentityDbContext<ApplicationUser>(options) 
    {
        public DbSet<Poll> Polls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Poll>()
            //    .Property(p => p.Title)
            //    .HasMaxLength(50);


            //modelBuilder.ApplyConfiguration(new PollConfiguration());
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }


    }
}
