using BIZ.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BIZ.Infrastructure.Persistence.Tenant;

public class TenantDbContextFactory
    : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();

        var connectionString =
            "Server=localhost,1433;" +
            "Database=BIZ_ERPDEMO1;" +
            "User Id=sa;" +
            "Password=BIZSql@2026Strong!;" +
            "TrustServerCertificate=True;" +
            "Encrypt=False";

        optionsBuilder.UseSqlServer(connectionString);

        var tenantContext = new DesignTimeTenantContext(
            companyId: 1,
            companyCode: "ERPDEMO1",
            companyName: "BIZ Demo Company",
            databaseServer: "localhost",
            databaseName: "BIZ_ERPDEMO1"
        );

        return new TenantDbContext(
            optionsBuilder.Options,
            tenantContext
        );
    }
}

internal class DesignTimeTenantContext : ITenantContext
{
    public DesignTimeTenantContext(
        int companyId,
        string companyCode,
        string companyName,
        string databaseServer,
        string databaseName)
    {
        CompanyId = companyId;
        CompanyCode = companyCode;
        CompanyName = companyName;
        DatabaseServer = databaseServer;
        DatabaseName = databaseName;
        IsResolved = true;
    }

    public int CompanyId { get; }

    public string CompanyCode { get; }

    public string CompanyName { get; }

    public string DatabaseServer { get; }

    public string DatabaseName { get; }

    public bool IsResolved { get; }

    public void SetTenant(
        int companyId,
        string companyCode,
        string companyName,
        string databaseServer,
        string databaseName)
    {
        throw new NotSupportedException(
            "Design-time tenant context is read-only."
        );
    }

    public void Clear()
    {
        throw new NotSupportedException(
            "Design-time tenant context is read-only."
        );
    }
}