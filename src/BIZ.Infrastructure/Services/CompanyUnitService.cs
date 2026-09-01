using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class CompanyUnitService : ICompanyUnitService
{
    private readonly TenantDbContext _db;

    public CompanyUnitService(TenantDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // Get All
    // ============================================================

    public async Task<List<CompanyUnit>> GetAllAsync()
    {
        return await _db.CompanyUnits
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    // ============================================================
    // Get By Id
    // ============================================================

    public async Task<CompanyUnit?> GetByIdAsync(int id)
    {
        return await _db.CompanyUnits
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    // ============================================================
    // Create
    // ============================================================

    public async Task<CompanyUnit> CreateAsync(
        CompanyUnit companyUnit)
    {
        companyUnit.Id = 0;
        companyUnit.CreatedAt = DateTime.UtcNow;
        companyUnit.UpdatedAt = null;
        companyUnit.IsActive = true;

        _db.CompanyUnits.Add(companyUnit);

        await _db.SaveChangesAsync();

        return companyUnit;
    }

    // ============================================================
    // Update
    // ============================================================

    public async Task<bool> UpdateAsync(
        int id,
        CompanyUnit companyUnit)
    {
        var existing = await _db.CompanyUnits
            .FirstOrDefaultAsync(x => x.Id == id);

        if (existing is null)
            return false;

        existing.Code = companyUnit.Code;
        existing.Name = companyUnit.Name;
        existing.Address = companyUnit.Address;
        existing.Phone = companyUnit.Phone;
        existing.Email = companyUnit.Email;
        existing.IsActive = companyUnit.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // Delete
    // ============================================================

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _db.CompanyUnits
            .FirstOrDefaultAsync(x => x.Id == id);

        if (existing is null)
            return false;

        _db.CompanyUnits.Remove(existing);

        await _db.SaveChangesAsync();

        return true;
    }
}