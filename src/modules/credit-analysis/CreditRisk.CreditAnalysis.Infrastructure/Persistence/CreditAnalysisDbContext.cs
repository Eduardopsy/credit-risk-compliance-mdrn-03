// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Persistence/CreditAnalysisDbContext.cs
using CreditRisk.CreditAnalysis.Domain.Entities;
using CreditRisk.Shared.Kernel.Domain;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CreditRisk.CreditAnalysis.Infrastructure.Persistence;

public sealed class CreditAnalysisDbContext(DbContextOptions<CreditAnalysisDbContext> options) : DbContext(options)
{
    public DbSet<CreditProposal> CreditProposals => Set<CreditProposal>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<DomainEvent>();
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
