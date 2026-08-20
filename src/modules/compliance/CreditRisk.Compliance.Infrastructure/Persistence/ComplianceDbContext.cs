// File: src/modules/compliance/CreditRisk.Compliance.Infrastructure/Persistence/ComplianceDbContext.cs
using CreditRisk.Compliance.Domain.Entities;
using CreditRisk.Shared.Kernel.Domain;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CreditRisk.Compliance.Infrastructure.Persistence;

public sealed class ComplianceDbContext(DbContextOptions<ComplianceDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<AmlAlert> AmlAlerts => Set<AmlAlert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<DomainEvent>();
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
