using BankingFraudDetector.Domain.FraudDetection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingFraudDetector.Infrastructure.Persistence.Configurations;

public sealed class AssessmentRecordConfiguration
    : IEntityTypeConfiguration<AssessmentRecord>
{
    public void Configure(EntityTypeBuilder<AssessmentRecord> builder)
    {
        builder.ToTable("AssessmentRecords");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.RiskBand)
            .HasConversion<int>();

        builder.Property(x => x.RuleSetVersion)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.AccountId,
            x.AssessedAtUtc
        });

        builder.HasMany(x => x.Findings)
            .WithOne()
            .HasForeignKey(x => x.AssessmentRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Findings)
            .HasField("_findings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}