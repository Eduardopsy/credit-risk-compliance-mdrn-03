// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Persistence/CreditAnalysisDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CreditRisk.CreditAnalysis.Infrastructure.Persistence;

public sealed class CreditAnalysisDbContextFactory : IDesignTimeDbContextFactory<CreditAnalysisDbContext>
{
    public CreditAnalysisDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=creditrisk;Username=crcl;Password=crcl;SearchPath=credit";

        var optionsBuilder = new DbContextOptionsBuilder<CreditAnalysisDbContext>();
        optionsBuilder.UseNpgsql(connectionString, o =>
        {
            o.MigrationsHistoryTable("__EFMigrationsHistory", "credit");
        });

        return new CreditAnalysisDbContext(optionsBuilder.Options);
    }
}
