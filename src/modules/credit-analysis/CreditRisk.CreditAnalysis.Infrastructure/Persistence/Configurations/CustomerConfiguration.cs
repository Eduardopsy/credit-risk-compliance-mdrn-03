// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Persistence/Configurations/CustomerConfiguration.cs
using CreditRisk.CreditAnalysis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreditRisk.CreditAnalysis.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(c => c.Document).HasColumnName("document").HasMaxLength(14).IsRequired();
        builder.Property(c => c.DocumentType).HasColumnName("document_type").HasMaxLength(10).IsRequired();
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(c => c.Email).HasColumnName("email").HasMaxLength(256).IsRequired();
        builder.Property(c => c.MonthlyIncome).HasColumnName("monthly_income").HasPrecision(18, 2).IsRequired();
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(c => c.Document).IsUnique().HasDatabaseName("ix_customers_document");
    }
}
