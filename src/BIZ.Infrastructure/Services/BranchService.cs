using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class BranchService : IBranchService
{
    private readonly TenantDbContext _db;

    public BranchService(TenantDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // Get All Branches
    // ============================================================

    public async Task<List<Branch>> GetAllAsync()
    {
        return await _db.Branches
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    // ============================================================
    // Get Branch By Id
    // ============================================================

    public async Task<Branch?> GetByIdAsync(int id)
    {
        return await _db.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    // ============================================================
    // Create Branch
    // ============================================================

    public async Task<Branch> CreateAsync(Branch branch)
    {
        branch.Id = 0;
        branch.CreatedAt = DateTime.UtcNow;
        branch.UpdatedAt = null;
        branch.IsActive = true;

        _db.Branches.Add(branch);

        await _db.SaveChangesAsync();

        return branch;
    }

    // ============================================================
    // Update Branch
    // ============================================================

    public async Task<bool> UpdateAsync(int id, Branch branch)
    {
        var existingBranch = await _db.Branches
            .FirstOrDefaultAsync(x => x.Id == id);

        if (existingBranch is null)
            return false;

        existingBranch.Code = branch.Code;
        existingBranch.Name = branch.Name;
        existingBranch.Address = branch.Address;
        existingBranch.Phone = branch.Phone;
        existingBranch.Email = branch.Email;
        existingBranch.IsActive = branch.IsActive;
        existingBranch.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // Delete Branch
    // ============================================================

    public async Task<bool> DeleteAsync(int id)
    {
        var branch = await _db.Branches
            .FirstOrDefaultAsync(x => x.Id == id);

        if (branch is null)
            return false;

        _db.Branches.Remove(branch);

        await _db.SaveChangesAsync();

        return true;
    }
}