using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class LedgerAccountService : ILedgerAccountService
{
    private readonly TenantDbContext _db;

    public LedgerAccountService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<LedgerAccountDto>> GetAllAsync()
    {
        return await _db.LedgerAccounts
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new LedgerAccountDto
            {
                Id = x.Id,
                AccountSubGroupId = x.AccountSubGroupId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                AccountType = x.AccountType,
                IsControlAccount = x.IsControlAccount,
                AllowManualEntry = x.AllowManualEntry,
                IsReconciliationRequired =
                    x.IsReconciliationRequired,
                OpeningDebit = x.OpeningDebit,
                OpeningCredit = x.OpeningCredit,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<LedgerAccountDto>>
        GetByAccountSubGroupAsync(
            int accountSubGroupId)
    {
        return await _db.LedgerAccounts
            .AsNoTracking()
            .Where(x =>
                x.AccountSubGroupId == accountSubGroupId)
            .OrderBy(x => x.Code)
            .Select(x => new LedgerAccountDto
            {
                Id = x.Id,
                AccountSubGroupId = x.AccountSubGroupId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                AccountType = x.AccountType,
                IsControlAccount = x.IsControlAccount,
                AllowManualEntry = x.AllowManualEntry,
                IsReconciliationRequired =
                    x.IsReconciliationRequired,
                OpeningDebit = x.OpeningDebit,
                OpeningCredit = x.OpeningCredit,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<LedgerAccountDto?> GetByIdAsync(int id)
    {
        return await _db.LedgerAccounts
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new LedgerAccountDto
            {
                Id = x.Id,
                AccountSubGroupId = x.AccountSubGroupId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                AccountType = x.AccountType,
                IsControlAccount = x.IsControlAccount,
                AllowManualEntry = x.AllowManualEntry,
                IsReconciliationRequired =
                    x.IsReconciliationRequired,
                OpeningDebit = x.OpeningDebit,
                OpeningCredit = x.OpeningCredit,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<LedgerAccountDto?> GetByCodeAsync(
        string code)
    {
        code = code.Trim().ToUpper();

        return await _db.LedgerAccounts
            .AsNoTracking()
            .Where(x => x.Code == code)
            .Select(x => new LedgerAccountDto
            {
                Id = x.Id,
                AccountSubGroupId = x.AccountSubGroupId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                AccountType = x.AccountType,
                IsControlAccount = x.IsControlAccount,
                AllowManualEntry = x.AllowManualEntry,
                IsReconciliationRequired =
                    x.IsReconciliationRequired,
                OpeningDebit = x.OpeningDebit,
                OpeningCredit = x.OpeningCredit,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<LedgerAccountDto> CreateAsync(
        LedgerAccountDto dto)
    {
        dto.Code = dto.Code.Trim().ToUpper();
        dto.Name = dto.Name.Trim();
        dto.AccountType = dto.AccountType.Trim();

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new ArgumentException(
                "Ledger Account Code is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException(
                "Ledger Account Name is required.");

        if (string.IsNullOrWhiteSpace(dto.AccountType))
            dto.AccountType = "General";

        if (dto.OpeningDebit < 0)
            throw new ArgumentException(
                "Opening Debit cannot be negative.");

        if (dto.OpeningCredit < 0)
            throw new ArgumentException(
                "Opening Credit cannot be negative.");

        if (dto.OpeningDebit > 0 &&
            dto.OpeningCredit > 0)
        {
            throw new ArgumentException(
                "Opening balance cannot have both Debit and Credit.");
        }

        var subGroup = await _db.AccountSubGroups
            .FirstOrDefaultAsync(x =>
                x.Id == dto.AccountSubGroupId &&
                x.IsActive);

        if (subGroup is null)
        {
            throw new ArgumentException(
                "Account Sub Group not found or inactive.");
        }

        var codeExists = await _db.LedgerAccounts
            .AnyAsync(x => x.Code == dto.Code);

        if (codeExists)
        {
            throw new InvalidOperationException(
                $"Ledger Account Code '{dto.Code}' already exists.");
        }

        var nameExists = await _db.LedgerAccounts
            .AnyAsync(x =>
                x.AccountSubGroupId ==
                    dto.AccountSubGroupId &&
                x.Name == dto.Name);

        if (nameExists)
        {
            throw new InvalidOperationException(
                $"Ledger Account Name '{dto.Name}' already exists under this Account Sub Group.");
        }

        var entity = new LedgerAccount
        {
            AccountSubGroupId = dto.AccountSubGroupId,
            Code = dto.Code,
            Name = dto.Name,
            Description =
                string.IsNullOrWhiteSpace(dto.Description)
                    ? null
                    : dto.Description.Trim(),
            AccountType = dto.AccountType,
            IsControlAccount = dto.IsControlAccount,
            AllowManualEntry = dto.AllowManualEntry,
            IsReconciliationRequired =
                dto.IsReconciliationRequired,
            OpeningDebit = dto.OpeningDebit,
            OpeningCredit = dto.OpeningCredit,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.LedgerAccounts.Add(entity);

        await _db.SaveChangesAsync();

        dto.Id = entity.Id;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        LedgerAccountDto dto)
    {
        var entity = await _db.LedgerAccounts
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        dto.Code = dto.Code.Trim().ToUpper();
        dto.Name = dto.Name.Trim();
        dto.AccountType = dto.AccountType.Trim();

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new ArgumentException(
                "Ledger Account Code is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException(
                "Ledger Account Name is required.");

        if (string.IsNullOrWhiteSpace(dto.AccountType))
            dto.AccountType = "General";

        if (dto.OpeningDebit < 0)
            throw new ArgumentException(
                "Opening Debit cannot be negative.");

        if (dto.OpeningCredit < 0)
            throw new ArgumentException(
                "Opening Credit cannot be negative.");

        if (dto.OpeningDebit > 0 &&
            dto.OpeningCredit > 0)
        {
            throw new ArgumentException(
                "Opening balance cannot have both Debit and Credit.");
        }

        var subGroup = await _db.AccountSubGroups
            .FirstOrDefaultAsync(x =>
                x.Id == dto.AccountSubGroupId &&
                x.IsActive);

        if (subGroup is null)
        {
            throw new ArgumentException(
                "Account Sub Group not found or inactive.");
        }

        var codeExists = await _db.LedgerAccounts
            .AnyAsync(x =>
                x.Id != id &&
                x.Code == dto.Code);

        if (codeExists)
        {
            throw new InvalidOperationException(
                $"Ledger Account Code '{dto.Code}' already exists.");
        }

        var nameExists = await _db.LedgerAccounts
            .AnyAsync(x =>
                x.Id != id &&
                x.AccountSubGroupId ==
                    dto.AccountSubGroupId &&
                x.Name == dto.Name);

        if (nameExists)
        {
            throw new InvalidOperationException(
                $"Ledger Account Name '{dto.Name}' already exists under this Account Sub Group.");
        }

        entity.AccountSubGroupId =
            dto.AccountSubGroupId;

        entity.Code = dto.Code;
        entity.Name = dto.Name;

        entity.Description =
            string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim();

        entity.AccountType = dto.AccountType;
        entity.IsControlAccount =
            dto.IsControlAccount;

        entity.AllowManualEntry =
            dto.AllowManualEntry;

        entity.IsReconciliationRequired =
            dto.IsReconciliationRequired;

        entity.OpeningDebit =
            dto.OpeningDebit;

        entity.OpeningCredit =
            dto.OpeningCredit;

        entity.IsActive =
            dto.IsActive;

        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.LedgerAccounts
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }
}