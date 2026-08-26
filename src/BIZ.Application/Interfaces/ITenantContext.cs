namespace BIZ.Application.Interfaces;

public interface ITenantContext
{
    int CompanyId { get; }

    string CompanyCode { get; }

    string CompanyName { get; }

    string DatabaseServer { get; }

    string DatabaseName { get; }

    bool IsResolved { get; }

    void SetTenant(
        int companyId,
        string companyCode,
        string companyName,
        string databaseServer,
        string databaseName);

    void Clear();
}