// File: src/modules/compliance/CreditRisk.Compliance.Infrastructure/Persistence/ComplianceDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CreditRisk.Compliance.Infrastructure.Persistence;

public sealed class ComplianceDbContextFactory : IDesignTimeDbContextFactory<ComplianceDbContext>
{
    public ComplianceDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=creditrisk;Username=crcl;Password=crcl;SearchPath=compliance";

        var optionsBuilder = new DbContextOptionsBuilder<ComplianceDbContext>();
        optionsBuilder.UseNpgsql(connectionString, o =>
        {
            o.MigrationsHistoryTable("__EFMigrationsHistory", "compliance");
        });

        return new ComplianceDbContext(optionsBuilder.Options);
    }
}
