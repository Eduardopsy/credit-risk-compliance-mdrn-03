// File: src/modules/iam/CreditRisk.IAM.Infrastructure/Persistence/IamDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CreditRisk.IAM.Infrastructure.Persistence;

public sealed class IamDbContextFactory : IDesignTimeDbContextFactory<IamDbContext>
{
    public IamDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=creditrisk;Username=crcl;Password=crcl;SearchPath=iam";

        var optionsBuilder = new DbContextOptionsBuilder<IamDbContext>();
        optionsBuilder.UseNpgsql(connectionString, o =>
        {
            o.MigrationsHistoryTable("__EFMigrationsHistory", "iam");
        });

        return new IamDbContext(optionsBuilder.Options);
    }
}
