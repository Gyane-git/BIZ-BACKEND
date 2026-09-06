using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BIZ.Infrastructure.Services;

public sealed class TenantDatabaseProvisioner
{
    private readonly IConfiguration _configuration;

    public TenantDatabaseProvisioner(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task ProvisionAsync(Company company)
    {
        var masterConnection = _configuration.GetConnectionString("MasterRegistry");
        if (string.IsNullOrWhiteSpace(masterConnection))
            throw new InvalidOperationException("MasterRegistry connection string is not configured.");

        var server = NormalizeServer(company.DatabaseServer);
        var masterBuilder = new SqlConnectionStringBuilder(masterConnection)
        {
            DataSource = server,
            InitialCatalog = "master"
        };

        await using (var connection = new SqlConnection(masterBuilder.ConnectionString))
        {
            await connection.OpenAsync();
            await using var existsCommand = connection.CreateCommand();
            existsCommand.CommandText = "SELECT DB_ID(@databaseName)";
            existsCommand.Parameters.AddWithValue("@databaseName", company.DatabaseName);
            var exists = await existsCommand.ExecuteScalarAsync();

            if (exists == null || exists == DBNull.Value)
            {
                var safeName = company.DatabaseName.Replace("]", "]]", StringComparison.Ordinal);
                await using var createCommand = connection.CreateCommand();
                createCommand.CommandText = $"CREATE DATABASE [{safeName}]";
                await createCommand.ExecuteNonQueryAsync();
            }
        }

        var tenantBuilder = new SqlConnectionStringBuilder(masterBuilder.ConnectionString)
        {
            InitialCatalog = company.DatabaseName
        };
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseSqlServer(tenantBuilder.ConnectionString)
            .Options;

        await using var tenantDb = new TenantDbContext(
            options,
            new ProvisioningTenantContext(company));
        await tenantDb.Database.MigrateAsync();
    }

    private static string NormalizeServer(string server)
    {
        var value = server?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Database server is required.");
        return value.Contains(',') || value.Contains('\\') ? value : $"{value},1433";
    }

    private sealed class ProvisioningTenantContext : ITenantContext
    {
        public ProvisioningTenantContext(Company company)
        {
            CompanyId = company.Id;
            CompanyCode = company.Code;
            CompanyName = company.Name;
            DatabaseServer = company.DatabaseServer;
            DatabaseName = company.DatabaseName;
            IsResolved = true;
        }

        public int CompanyId { get; }
        public string CompanyCode { get; }
        public string CompanyName { get; }
        public string DatabaseServer { get; }
        public string DatabaseName { get; }
        public bool IsResolved { get; }
        public void SetTenant(int companyId, string companyCode, string companyName, string databaseServer, string databaseName) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
    }
}
