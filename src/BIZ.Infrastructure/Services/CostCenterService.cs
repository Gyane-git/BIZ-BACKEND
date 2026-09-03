using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class CostCenterService : ICostCenterService
{
    private readonly TenantDbContext _context;

    public CostCenterService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<List<CostCenterDto>> GetAllAsync()
    {
        return await _context.CostCenters
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new CostCenterDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                CompanyUnitId = x.CompanyUnitId,
                BranchId = x.BranchId,
                DepartmentId = x.DepartmentId,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<CostCenterDto?> GetByIdAsync(int id)
    {
        return await _context.CostCenters
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new CostCenterDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                CompanyUnitId = x.CompanyUnitId,
                BranchId = x.BranchId,
                DepartmentId = x.DepartmentId,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CostCenterDto?> GetByCodeAsync(string code)
    {
        code = code.Trim().ToUpper();

        return await _context.CostCenters
            .AsNoTracking()
            .Where(x => x.Code == code && x.IsActive)
            .Select(x => new CostCenterDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                CompanyUnitId = x.CompanyUnitId,
                BranchId = x.BranchId,
                DepartmentId = x.DepartmentId,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<CostCenterDto>> GetByBranchAsync(int branchId)
    {
        return await _context.CostCenters
            .AsNoTracking()
            .Where(x =>
                x.BranchId == branchId &&
                x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new CostCenterDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                CompanyUnitId = x.CompanyUnitId,
                BranchId = x.BranchId,
                DepartmentId = x.DepartmentId,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<CostCenterDto>> GetByDepartmentAsync(
        int departmentId)
    {
        return await _context.CostCenters
            .AsNoTracking()
            .Where(x =>
                x.DepartmentId == departmentId &&
                x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new CostCenterDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                CompanyUnitId = x.CompanyUnitId,
                BranchId = x.BranchId,
                DepartmentId = x.DepartmentId,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<CostCenterDto> CreateAsync(
        CostCenterDto dto)
    {
        var code = dto.Code.Trim().ToUpper();
        var name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(
                "CostCenter code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "CostCenter name is required.");

        if (dto.CompanyUnitId.HasValue)
        {
            var companyUnitExists =
                await _context.CompanyUnits
                    .AnyAsync(x =>
                        x.Id == dto.CompanyUnitId.Value &&
                        x.IsActive);

            if (!companyUnitExists)
                throw new ArgumentException(
                    "Active CompanyUnit not found.");
        }

        if (dto.BranchId.HasValue)
        {
            var branchExists =
                await _context.Branches
                    .AnyAsync(x =>
                        x.Id == dto.BranchId.Value &&
                        x.IsActive);

            if (!branchExists)
                throw new ArgumentException(
                    "Active Branch not found.");
        }

        if (dto.DepartmentId.HasValue)
        {
            var departmentExists =
                await _context.Departments
                    .AnyAsync(x =>
                        x.Id == dto.DepartmentId.Value &&
                        x.IsActive);

            if (!departmentExists)
                throw new ArgumentException(
                    "Active Department not found.");
        }

        var codeExists = await _context.CostCenters
            .AnyAsync(x => x.Code == code);

        if (codeExists)
            throw new InvalidOperationException(
                $"CostCenter code '{code}' already exists.");

        var nameExists = await _context.CostCenters
            .AnyAsync(x => x.Name == name);

        if (nameExists)
            throw new InvalidOperationException(
                $"CostCenter name '{name}' already exists.");

        var entity = new CostCenter
        {
            Code = code,
            Name = name,
            Description = dto.Description?.Trim(),
            CompanyUnitId = dto.CompanyUnitId,
            BranchId = dto.BranchId,
            DepartmentId = dto.DepartmentId,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.CostCenters.Add(entity);

        await _context.SaveChangesAsync();

        return new CostCenterDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            CompanyUnitId = entity.CompanyUnitId,
            BranchId = entity.BranchId,
            DepartmentId = entity.DepartmentId,
            IsActive = entity.IsActive
        };
    }

    public async Task<bool> UpdateAsync(
        int id,
        CostCenterDto dto)
    {
        var entity = await _context.CostCenters
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (entity == null)
            return false;

        var code = dto.Code.Trim().ToUpper();
        var name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(
                "CostCenter code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "CostCenter name is required.");

        if (dto.CompanyUnitId.HasValue)
        {
            var exists = await _context.CompanyUnits
                .AnyAsync(x =>
                    x.Id == dto.CompanyUnitId.Value &&
                    x.IsActive);

            if (!exists)
                throw new ArgumentException(
                    "Active CompanyUnit not found.");
        }

        if (dto.BranchId.HasValue)
        {
            var exists = await _context.Branches
                .AnyAsync(x =>
                    x.Id == dto.BranchId.Value &&
                    x.IsActive);

            if (!exists)
                throw new ArgumentException(
                    "Active Branch not found.");
        }

        if (dto.DepartmentId.HasValue)
        {
            var exists = await _context.Departments
                .AnyAsync(x =>
                    x.Id == dto.DepartmentId.Value &&
                    x.IsActive);

            if (!exists)
                throw new ArgumentException(
                    "Active Department not found.");
        }

        var codeExists = await _context.CostCenters
            .AnyAsync(x =>
                x.Id != id &&
                x.Code == code);

        if (codeExists)
            throw new InvalidOperationException(
                $"CostCenter code '{code}' already exists.");

        var nameExists = await _context.CostCenters
            .AnyAsync(x =>
                x.Id != id &&
                x.Name == name);

        if (nameExists)
            throw new InvalidOperationException(
                $"CostCenter name '{name}' already exists.");

        entity.Code = code;
        entity.Name = name;
        entity.Description = dto.Description?.Trim();
        entity.CompanyUnitId = dto.CompanyUnitId;
        entity.BranchId = dto.BranchId;
        entity.DepartmentId = dto.DepartmentId;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.CostCenters
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (entity == null)
            return false;

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}