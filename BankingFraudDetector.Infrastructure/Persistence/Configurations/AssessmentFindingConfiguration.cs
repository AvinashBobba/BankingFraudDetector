using BankingFraudDetector.Domain.FraudDetection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingFraudDetector.Infrastructure.Persistence.Configurations;

public sealed class AssessmentFindingConfiguration
    : IEntityTypeConfiguration<AssessmentFinding>
{
    public void Configure(EntityTypeBuilder<AssessmentFinding> builder)
    {
        builder.ToTable("AssessmentFindings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.RuleCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Explanation)
            .HasMaxLength(2000)
            .IsRequired();

        builder.PrimitiveCollection(x => x.EvidenceEventIds)
            .HasColumnType("nvarchar(max)")
            .IsRequired();
    }
}