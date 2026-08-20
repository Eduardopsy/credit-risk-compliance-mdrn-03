// File: src/modules/compliance/CreditRisk.Compliance.Infrastructure/Persistence/Configurations/TransactionConfiguration.cs
using CreditRisk.Compliance.Domain.Entities;
using CreditRisk.Compliance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreditRisk.Compliance.Infrastructure.Persistence.Configurations;

internal sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(t => t.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(t => t.TransactionType).HasColumnName("transaction_type").HasMaxLength(20).IsRequired();
        builder.Property(t => t.Channel).HasColumnName("channel").HasMaxLength(20).IsRequired();
        builder.Property(t => t.TransactionDate).HasColumnName("transaction_date").IsRequired();
        builder.Property(t => t.OriginAccountId).HasColumnName("origin_account_id").HasMaxLength(50).IsRequired();
        builder.Property(t => t.DestinationAccountId).HasColumnName("destination_account_id").HasMaxLength(50).IsRequired();
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<TransactionStatus>(v))
            .IsRequired();

        builder.OwnsOne(t => t.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
            money.Ignore(m => m.Currency);
        });

        builder.HasIndex(t => t.CustomerId).HasDatabaseName("ix_transactions_customer_id");
        builder.HasIndex(t => t.TransactionDate).HasDatabaseName("ix_transactions_transaction_date");
    }
}
