
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SurveyBasket.Api.Persistence.EntitiesConfigurations;
using System.Security.Claims;

namespace SurveyBasket.Api.Persistence
{
    // Prime Constractors feature used here (C# 9.0 and later)
    // u should here the application for Identity user and ause ApplicationRole for the role,
    // and use string as the primary key type for both user and role (u can modify the type of the primary key)
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,IHttpContextAccessor httpContextAccessor)
        : IdentityDbContext<ApplicationUser, ApplicationRole,string>(options)
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public DbSet<Poll> Polls { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<VoteAnswer> VoteAnswers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Configrations
            //modelBuilder.Entity<Poll>()
            //    .Property(p => p.Title)
            //    .HasMaxLength(50);
            //modelBuilder.ApplyConfiguration(new PollConfiguration());

            #endregion

            var cascadeFKs= modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade && !fk.IsOwnership);

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<AuditableEntity>();
            foreach (var entityEntry in entries)
            {
                var currentUserId = _httpContextAccessor.HttpContext?.User?.GetUserId()!;
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
