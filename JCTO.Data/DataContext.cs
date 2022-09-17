using JCTO.Domain;
using JCTO.Domain.CustomExceptions;
using JCTO.Domain.Dtos;
using JCTO.Domain.Entities;
using JCTO.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace JCTO.Data
{
    public class DataContext : DbContext, IDataContext
    {
        private readonly IUserContext _userContext;

        public DataContext(DbContextOptions<DataContext> options, IUserContext userContext)
           : base(options)
        {
            _userContext = userContext;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //Sequences
            if (Database.IsNpgsql())
            {
                builder.HasSequence<int>("EntryIndex").StartsAt(1).IncrementsBy(1);
                builder.Entity<Entry>().Property(h => h.Index).HasDefaultValueSql("nextval('\"EntryIndex\"')");
                builder.Entity<Entry>().HasIndex(t => t.Index).IsUnique();
            }

            //Indexes
            builder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            builder.Entity<Customer>().HasIndex(c => c.Name).IsUnique();
            builder.Entity<Product>().HasIndex(p => p.Code).IsUnique();
            builder.Entity<Entry>().HasIndex(e => e.EntryNo).IsUnique();
            builder.Entity<Entry>().HasIndex(t => t.StockTransactionId).IsUnique();
            builder.Entity<EntryTransaction>().HasIndex(t => new { t.ApprovalType, t.ApprovalRef }).HasFilter("\"Type\" = 0 AND \"ApprovalType\" in (1,2)");
            builder.Entity<Order>().HasIndex(o => new { o.OrderDate, o.OrderNo }).IsUnique();
            builder.Entity<Stock>().HasIndex(s => new { s.CustomerId, s.ProductId }).IsUnique();
            builder.Entity<StockTransaction>().HasIndex(t => t.ToBondNo).IsUnique();


            foreach (var property in builder.Model.GetEntityTypes()
                 .SelectMany(t => t.GetProperties())
                 .Where(p => (p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)) && !p.Name.EndsWith("Utc")))
            {
                property.SetColumnType("timestamp without time zone");
            }
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<EntryTransaction> EntryTransactions { get; set; }
        public DbSet<BowserEntry> BowserEntries { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
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
                if (entry.OriginalValues["ConcurrencyKey"].ToString() != entry.Entity.ConcurrencyKey.ToString())
                {
                    throw new JCTOConcurrencyException(entry.Entity.ToString()!);
                }

                entry.Entity.ConcurrencyKey = Guid.NewGuid();
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}