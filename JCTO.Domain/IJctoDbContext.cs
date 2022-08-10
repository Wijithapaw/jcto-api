using JCTO.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JCTO.Domain
{
    public interface IJctoDbContext : IDisposable
    {
        DbSet<User> Users { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
