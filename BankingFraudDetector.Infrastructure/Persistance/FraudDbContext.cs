using BankingFraudDetector.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace BankingFraudDetector.Infrastructure.Persistence;

public sealed class FraudDbContext : DbContext
{
    public FraudDbContext(DbContextOptions<FraudDbContext> options)
        : base(options)
    {
    }

    public DbSet<AccountEvent> AccountEvents => Set<AccountEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FraudDbContext).Assembly);
    }
}