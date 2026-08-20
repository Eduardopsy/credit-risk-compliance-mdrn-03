// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Persistence/Configurations/CreditProposalConfiguration.cs
using CreditRisk.CreditAnalysis.Domain.Entities;
using CreditRisk.CreditAnalysis.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreditRisk.CreditAnalysis.Infrastructure.Persistence.Configurations;

internal sealed class CreditProposalConfiguration : IEntityTypeConfiguration<CreditProposal>
{
    public void Configure(EntityTypeBuilder<CreditProposal> builder)
    {
        builder.ToTable("credit_proposals");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(p => p.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(p => p.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(p => p.ProposalType).HasColumnName("proposal_type").HasMaxLength(20).IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<ProposalStatus>(v))
            .IsRequired();

        builder.Property(p => p.Rating)
            .HasColumnName("risk_rating")
            .HasMaxLength(1)
            .HasConversion(
                v => v.HasValue ? v.Value.ToString() : null,
                v => v != null ? Enum.Parse<RiskRating>(v) : (RiskRating?)null);

        builder.OwnsOne(p => p.RequestedLimit, money =>
        {
            money.Property(m => m.Amount).HasColumnName("requested_limit").HasPrecision(18, 2).IsRequired();
            money.Ignore(m => m.Currency);
        });

        builder.OwnsOne(p => p.ApprovedLimit, money =>
        {
            money.Property(m => m.Amount).HasColumnName("approved_limit").HasPrecision(18, 2);
            money.Ignore(m => m.Currency);
        });

        builder.Property(p => p.RequiresManualReview).HasColumnName("requires_manual_review").IsRequired();

        builder.HasIndex(p => p.CustomerId).HasDatabaseName("ix_credit_proposals_customer_id");
        builder.HasIndex(p => p.Status).HasDatabaseName("ix_credit_proposals_status");
        builder.HasIndex(p => p.CreatedAt).HasDatabaseName("ix_credit_proposals_created_at");
    }
}
