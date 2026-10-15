using Microsoft.EntityFrameworkCore;

namespace Savor.Infrastructure.Persistence;

public sealed class SavorDbContext(DbContextOptions<SavorDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SavorDbContext).Assembly);
    }
}
