// File: src/modules/iam/CreditRisk.IAM.Infrastructure/Persistence/Configurations/OutboxMessageConfiguration.cs
using CreditRisk.Shared.Kernel.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreditRisk.IAM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for OutboxMessage.
/// Maps to a database table that stores messages for guaranteed delivery via Outbox Pattern.
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MessageType)
            .HasColumnName("message_type")
            .HasColumnType("varchar(500)")
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.ScheduledAt)
            .HasColumnName("scheduled_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.RetryCount)
            .HasColumnName("retry_count")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.Error)
            .HasColumnName("error")
            .HasColumnType("text");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Indexes for query performance
        builder.HasIndex(x => x.ProcessedAt).HasDatabaseName("idx_outbox_processed_at");
        builder.HasIndex(x => new { x.ScheduledAt, x.ProcessedAt }).HasDatabaseName("idx_outbox_unprocessed");
    }
}
