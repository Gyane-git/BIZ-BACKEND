using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class AccountSubGroupService : IAccountSubGroupService
{
    private readonly TenantDbContext _db;

    public AccountSubGroupService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<AccountSubGroupDto>> GetAllAsync()
    {
        return await _db.AccountSubGroups
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new AccountSubGroupDto
            {
                Id = x.Id,
                AccountGroupId = x.AccountGroupId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<AccountSubGroupDto>>
        GetByAccountGroupAsync(int accountGroupId)
    {
        return await _db.AccountSubGroups
            .AsNoTracking()
            .Where(x =>
                x.AccountGroupId == accountGroupId)
            .OrderBy(x => x.Code)
            .Select(x => new AccountSubGroupDto
            {
                Id = x.Id,
                AccountGroupId = x.AccountGroupId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<AccountSubGroupDto?> GetByIdAsync(int id)
    {
        return await _db.AccountSubGroups
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AccountSubGroupDto
            {
                Id = x.Id,
                AccountGroupId = x.AccountGroupId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AccountSubGroupDto?> GetByCodeAsync(
        string code)
    {
        code = code.Trim().ToUpper();

        return await _db.AccountSubGroups
            .AsNoTracking()
            .Where(x => x.Code == code)
            .Select(x => new AccountSubGroupDto
            {
                Id = x.Id,
                AccountGroupId = x.AccountGroupId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AccountSubGroupDto> CreateAsync(
        AccountSubGroupDto dto)
    {
        dto.Code = dto.Code.Trim().ToUpper();
        dto.Name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new ArgumentException(
                "Account Sub Group Code is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException(
                "Account Sub Group Name is required.");

        var accountGroup = await _db.AccountGroups
            .FirstOrDefaultAsync(x =>
                x.Id == dto.AccountGroupId &&
                x.IsActive);

        if (accountGroup is null)
        {
            throw new ArgumentException(
                "Account Group not found or inactive.");
        }

        var codeExists = await _db.AccountSubGroups
            .AnyAsync(x => x.Code == dto.Code);

        if (codeExists)
        {
            throw new InvalidOperationException(
                $"Account Sub Group Code '{dto.Code}' already exists.");
        }

        var nameExists = await _db.AccountSubGroups
            .AnyAsync(x =>
                x.AccountGroupId == dto.AccountGroupId &&
                x.Name == dto.Name);

        if (nameExists)
        {
            throw new InvalidOperationException(
                $"Account Sub Group Name '{dto.Name}' already exists under this Account Group.");
        }

        var entity = new AccountSubGroup
        {
            AccountGroupId = dto.AccountGroupId,
            Code = dto.Code,
            Name = dto.Name,
            Description = string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.AccountSubGroups.Add(entity);

        await _db.SaveChangesAsync();

        dto.Id = entity.Id;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        AccountSubGroupDto dto)
    {
        var entity = await _db.AccountSubGroups
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        dto.Code = dto.Code.Trim().ToUpper();
        dto.Name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new ArgumentException(
                "Account Sub Group Code is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException(
                "Account Sub Group Name is required.");

        var accountGroup = await _db.AccountGroups
            .FirstOrDefaultAsync(x =>
                x.Id == dto.AccountGroupId &&
                x.IsActive);

        if (accountGroup is null)
        {
            throw new ArgumentException(
                "Account Group not found or inactive.");
        }

        var codeExists = await _db.AccountSubGroups
            .AnyAsync(x =>
                x.Id != id &&
                x.Code == dto.Code);

        if (codeExists)
        {
            throw new InvalidOperationException(
                $"Account Sub Group Code '{dto.Code}' already exists.");
        }

        var nameExists = await _db.AccountSubGroups
            .AnyAsync(x =>
                x.Id != id &&
                x.AccountGroupId == dto.AccountGroupId &&
                x.Name == dto.Name);

        if (nameExists)
        {
            throw new InvalidOperationException(
                $"Account Sub Group Name '{dto.Name}' already exists under this Account Group.");
        }

        entity.AccountGroupId = dto.AccountGroupId;
        entity.Code = dto.Code;
        entity.Name = dto.Name;
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
        var entity = await _db.AccountSubGroups
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }
}