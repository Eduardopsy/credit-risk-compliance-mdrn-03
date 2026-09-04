// File: src/modules/iam/CreditRisk.IAM.Infrastructure/Persistence/IamDbContext.cs
using CreditRisk.IAM.Domain.Entities;
using CreditRisk.Shared.Kernel.Domain;
using CreditRisk.Shared.Kernel.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CreditRisk.IAM.Infrastructure.Persistence;

public sealed class IamDbContext(DbContextOptions<IamDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<DomainEvent>();
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
