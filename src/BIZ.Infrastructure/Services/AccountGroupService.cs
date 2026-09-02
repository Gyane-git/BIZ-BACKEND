using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class AccountGroupService : IAccountGroupService
{
    private readonly TenantDbContext _db;

    public AccountGroupService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<AccountGroupDto>> GetAllAsync()
    {
        return await _db.AccountGroups
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new AccountGroupDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Nature = x.Nature,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<AccountGroupDto?> GetByIdAsync(int id)
    {
        return await _db.AccountGroups
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AccountGroupDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Nature = x.Nature,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AccountGroupDto?> GetByCodeAsync(string code)
    {
        code = code.Trim();

        return await _db.AccountGroups
            .AsNoTracking()
            .Where(x => x.Code == code)
            .Select(x => new AccountGroupDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Nature = x.Nature,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AccountGroupDto> CreateAsync(AccountGroupDto dto)
    {
        dto.Code = dto.Code.Trim().ToUpper();
        dto.Name = dto.Name.Trim();
        dto.Nature = dto.Nature.Trim();

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new ArgumentException("Account Group Code is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Account Group Name is required.");

        var allowedNatures = new[]
        {
            "Asset",
            "Liability",
            "Equity",
            "Revenue",
            "Expense"
        };

        if (!allowedNatures.Contains(
                dto.Nature,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Invalid Nature. Allowed values: Asset, Liability, Equity, Revenue, Expense.");
        }

        var codeExists = await _db.AccountGroups
            .AnyAsync(x => x.Code == dto.Code);

        if (codeExists)
            throw new InvalidOperationException(
                $"Account Group Code '{dto.Code}' already exists.");

        var nameExists = await _db.AccountGroups
            .AnyAsync(x => x.Name == dto.Name);

        if (nameExists)
            throw new InvalidOperationException(
                $"Account Group Name '{dto.Name}' already exists.");

        var entity = new AccountGroup
        {
            Code = dto.Code,
            Name = dto.Name,
            Nature = dto.Nature,
            Description = string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.AccountGroups.Add(entity);

        await _db.SaveChangesAsync();

        dto.Id = entity.Id;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        AccountGroupDto dto)
    {
        var entity = await _db.AccountGroups
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        dto.Code = dto.Code.Trim().ToUpper();
        dto.Name = dto.Name.Trim();
        dto.Nature = dto.Nature.Trim();

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new ArgumentException("Account Group Code is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Account Group Name is required.");

        var allowedNatures = new[]
        {
            "Asset",
            "Liability",
            "Equity",
            "Revenue",
            "Expense"
        };

        if (!allowedNatures.Contains(
                dto.Nature,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Invalid Nature. Allowed values: Asset, Liability, Equity, Revenue, Expense.");
        }

        var codeExists = await _db.AccountGroups
            .AnyAsync(x =>
                x.Id != id &&
                x.Code == dto.Code);

        if (codeExists)
            throw new InvalidOperationException(
                $"Account Group Code '{dto.Code}' already exists.");

        var nameExists = await _db.AccountGroups
            .AnyAsync(x =>
                x.Id != id &&
                x.Name == dto.Name);

        if (nameExists)
            throw new InvalidOperationException(
                $"Account Group Name '{dto.Name}' already exists.");

        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.Nature = dto.Nature;
        entity.Description = string.IsNullOrWhiteSpace(dto.Description)
            ? null
            : dto.Description.Trim();
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.AccountGroups
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }
}