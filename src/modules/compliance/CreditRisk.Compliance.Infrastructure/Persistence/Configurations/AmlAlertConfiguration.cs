// File: src/modules/compliance/CreditRisk.Compliance.Infrastructure/Persistence/Configurations/AmlAlertConfiguration.cs
using CreditRisk.Compliance.Domain.Entities;
using CreditRisk.Compliance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreditRisk.Compliance.Infrastructure.Persistence.Configurations;

internal sealed class AmlAlertConfiguration : IEntityTypeConfiguration<AmlAlert>
{
    public void Configure(EntityTypeBuilder<AmlAlert> builder)
    {
        builder.ToTable("aml_alerts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(a => a.TransactionId).HasColumnName("transaction_id").IsRequired();
        builder.Property(a => a.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(a => a.AlertType).HasColumnName("alert_type").HasMaxLength(50).IsRequired();
        builder.Property(a => a.TransactionAmount).HasColumnName("transaction_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(a => a.ReviewedBy).HasColumnName("reviewed_by").HasMaxLength(100);
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.Property(a => a.Severity)
            .HasColumnName("severity")
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<AlertSeverity>(v))
            .IsRequired();

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<AlertStatus>(v))
            .IsRequired();

        builder.HasIndex(a => a.TransactionId).HasDatabaseName("ix_aml_alerts_transaction_id");
        builder.HasIndex(a => a.Status).HasDatabaseName("ix_aml_alerts_status");
    }
}
