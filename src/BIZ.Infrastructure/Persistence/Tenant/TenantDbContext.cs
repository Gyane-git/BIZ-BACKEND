using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Persistence.Tenant;

public class TenantDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<TenantTest> TenantTests => Set<TenantTest>();

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        if (!_tenantContext.IsResolved)
        {
            throw new InvalidOperationException(
                "Tenant has not been resolved."
            );
        }

        var connectionString =
            $"Server={_tenantContext.DatabaseServer},1433;" +
            $"Database={_tenantContext.DatabaseName};" +
            $"User Id=sa;" +
            $"Password=BIZSql@2026Strong!;" +
            $"TrustServerCertificate=True;" +
            $"Encrypt=False";

        optionsBuilder.UseSqlServer(connectionString);
    }
}