
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SurveyBasket.Api.Persistence.EntitiesConfigurations;
using System.Security.Claims;

namespace SurveyBasket.Api.Persistence
{
    // Prime Constractors feature used here (C# 9.0 and later)
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,IHttpContextAccessor httpContextAccessor)
        : IdentityDbContext<ApplicationUser>(options)
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

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

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<AuditableEntity>();
            foreach (var entityEntry in entries)
            {
                var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)!;
                if (entityEntry.State == EntityState.Added)
                {
                    entityEntry.Property(x => x.CreatedById).CurrentValue = currentUserId; // Replace with actual user ID
                }
                else if (entityEntry.State == EntityState.Modified)
                {
                    entityEntry.Property(x=>x.UpdatedById).CurrentValue = currentUserId; // Replace with actual user ID
                    entityEntry.Property(x=>x.UpdatedOn).CurrentValue = DateTime.UtcNow;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
