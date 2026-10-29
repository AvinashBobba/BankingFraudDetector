using BankingFraudDetector.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingFraudDetector.Infrastructure.Persistence.Configurations;

public sealed class AccountEventConfiguration
    : IEntityTypeConfiguration<AccountEvent>
{
    public void Configure(EntityTypeBuilder<AccountEvent> builder)
    {
        builder.ToTable("AccountEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.AccountId)
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.OccurredAtUtc)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.AccountId,
            x.OccurredAtUtc
        });
    }
}