// File: src/modules/iam/CreditRisk.IAM.Infrastructure/Persistence/IamDbContext.cs
using CreditRisk.IAM.Domain.Entities;
using CreditRisk.Shared.Kernel.Domain;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CreditRisk.IAM.Infrastructure.Persistence;

public sealed class IamDbContext(DbContextOptions<IamDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<DomainEvent>();
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
