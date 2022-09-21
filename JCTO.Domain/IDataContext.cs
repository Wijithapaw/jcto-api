using JCTO.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JCTO.Domain
{
    public interface IDataContext : IDisposable
    {
        DbSet<User> Users { get; set; }
        DbSet<Customer> Customers { get; set; }
        DbSet<Product> Products { get; set; }
        DbSet<Entry> Entries { get; set; }
        DbSet<EntryTransaction> EntryTransactions { get; set; }
        DbSet<BowserEntry> BowserEntries { get; set; }
        DbSet<Order> Orders { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
