// File: src/modules/iam/CreditRisk.IAM.Infrastructure/Persistence/Configurations/UserConfiguration.cs
using CreditRisk.IAM.Domain.Entities;
using CreditRisk.IAM.Domain.Enums;
using CreditRisk.IAM.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreditRisk.IAM.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id").ValueGeneratedNever();

        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("email").HasMaxLength(256).IsRequired();
            email.HasIndex(e => e.Value).IsUnique().HasDatabaseName("ix_users_email");
        });

        builder.Property(u => u.FullName).HasColumnName("full_name").HasMaxLength(150).IsRequired();

        builder.OwnsOne(u => u.PasswordHash, pwd =>
        {
            pwd.Property(p => p.Value).HasColumnName("password_hash").HasMaxLength(256).IsRequired();
        });

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<UserRole>(v))
            .IsRequired();

        builder.Property(u => u.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at").IsRequired();
    }
}
