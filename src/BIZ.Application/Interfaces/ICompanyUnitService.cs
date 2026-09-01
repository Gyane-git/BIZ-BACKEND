using BIZ.Domain.Entities;

namespace BIZ.Application.Interfaces;

public interface ICompanyUnitService
{
    Task<List<CompanyUnit>> GetAllAsync();

    Task<CompanyUnit?> GetByIdAsync(int id);

    Task<CompanyUnit> CreateAsync(CompanyUnit companyUnit);

    Task<bool> UpdateAsync(int id, CompanyUnit companyUnit);

    Task<bool> DeleteAsync(int id);
}