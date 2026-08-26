using BIZ.Application.Interfaces;

namespace BIZ.Infrastructure.Tenant;

public class TenantContext : ITenantContext
{
    public int CompanyId { get; private set; }

    public string CompanyCode { get; private set; } = string.Empty;

    public string CompanyName { get; private set; } = string.Empty;

    public string DatabaseServer { get; private set; } = string.Empty;

    public string DatabaseName { get; private set; } = string.Empty;

    public bool IsResolved { get; private set; }

    public void SetTenant(
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

    public void Clear()
    {
        CompanyId = 0;
        CompanyCode = string.Empty;
        CompanyName = string.Empty;
        DatabaseServer = string.Empty;
        DatabaseName = string.Empty;
        IsResolved = false;
    }
}