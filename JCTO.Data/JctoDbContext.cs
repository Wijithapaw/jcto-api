using JCTO.Domain;
using JCTO.Domain.CustomExceptions;
using JCTO.Domain.Dtos;
using JCTO.Domain.Entities;
using JCTO.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace JCTO.Data
{
    public class JctoDbContext : DbContext, IJctoDbContext 
    {
        private readonly IUserContext _userContext;

        public JctoDbContext(DbContextOptions<JctoDbContext> options, IUserContext userContext)
           : base(options)
        {
            _userContext = userContext;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //Indexes
            builder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        }

        public DbSet<User> Users { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_userContext != null)
            {
                foreach (var entry in ChangeTracker.Entries<IAuditedEntity>().Where(e => e.State == EntityState.Added))
                {
                    entry.Entity.CreatedById = entry.Entity.LastUpdatedById = Guid.Parse(_userContext.UserId!);
                    entry.Entity.CreatedDateUtc = entry.Entity.LastUpdatedDateUtc = DateTime.UtcNow;
                }

                foreach (var entry in ChangeTracker.Entries<IAuditedEntity>().Where(e => e.State == EntityState.Modified))
                {
                    entry.Entity.LastUpdatedById = Guid.Parse(_userContext.UserId!);
                    entry.Entity.LastUpdatedDateUtc = DateTime.UtcNow;
                }
            }

            foreach (var entry in ChangeTracker.Entries<IConcurrencyHandledEntity>().Where(e => e.State == EntityState.Added))
            {
                entry.Entity.ConcurrencyKey = Guid.NewGuid();
            }

            foreach (var entry in ChangeTracker.Entries<IConcurrencyHandledEntity>().Where(e => e.State == EntityState.Modified))
            {
                if (entry.OriginalValues["ConcurrencyKey"]?.ToString() != entry.Entity.ConcurrencyKey?.ToString())
                {
                    throw new JCTOConcurrencyException(entry.Entity.ToString()!);
                }
                
                entry.Entity.ConcurrencyKey = Guid.NewGuid();
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}