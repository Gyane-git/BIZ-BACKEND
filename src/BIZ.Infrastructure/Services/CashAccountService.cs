using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class CashAccountService : ICashAccountService
{
    private readonly TenantDbContext _context;

    public CashAccountService(
        TenantDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // GET ALL
    // =========================================================

    public async Task<List<CashAccountDto>> GetAllAsync()
    {
        return await _context.CashAccounts
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CashAccountDto
            {
                Id = x.Id,
                LedgerAccountId = x.LedgerAccountId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                OpeningBalance = x.OpeningBalance,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }


    // =========================================================
    // GET BY ID
    // =========================================================

    public async Task<CashAccountDto?> GetByIdAsync(
        int id)
    {
        return await _context.CashAccounts
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CashAccountDto
            {
                Id = x.Id,
                LedgerAccountId = x.LedgerAccountId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                OpeningBalance = x.OpeningBalance,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }


    // =========================================================
    // GET BY CODE
    // =========================================================

    public async Task<CashAccountDto?> GetByCodeAsync(
        string code)
    {
        code = code.Trim().ToUpper();

        return await _context.CashAccounts
            .AsNoTracking()
            .Where(x => x.Code == code)
            .Select(x => new CashAccountDto
            {
                Id = x.Id,
                LedgerAccountId = x.LedgerAccountId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                OpeningBalance = x.OpeningBalance,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }


    // =========================================================
    // CREATE
    // =========================================================

    public async Task<CashAccountDto> CreateAsync(
        CashAccountDto dto)
    {
        dto.Code = dto.Code.Trim().ToUpper();
        dto.Name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new Exception(
                "Code is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new Exception(
                "Name is required.");

        if (dto.LedgerAccountId <= 0)
            throw new Exception(
                "Valid LedgerAccountId is required.");

        if (dto.OpeningBalance < 0)
            throw new Exception(
                "OpeningBalance cannot be negative.");

        // ---------------------------------------------------------
        // Ledger Account validation
        // ---------------------------------------------------------

        var ledgerAccount = await _context.LedgerAccounts
            .FirstOrDefaultAsync(x =>
                x.Id == dto.LedgerAccountId &&
                x.IsActive);

        if (ledgerAccount == null)
            throw new Exception(
                "Ledger Account not found.");

        // ---------------------------------------------------------
        // Code duplicate
        // ---------------------------------------------------------

        var codeExists = await _context.CashAccounts
            .AnyAsync(x => x.Code == dto.Code);

        if (codeExists)
            throw new Exception(
                "Cash Account code already exists.");

        // ---------------------------------------------------------
        // Name duplicate
        // ---------------------------------------------------------

        var nameExists = await _context.CashAccounts
            .AnyAsync(x => x.Name == dto.Name);

        if (nameExists)
            throw new Exception(
                "Cash Account name already exists.");

        // ---------------------------------------------------------
        // One Cash Account per Ledger Account
        // ---------------------------------------------------------

        var ledgerCashExists = await _context.CashAccounts
            .AnyAsync(x =>
                x.LedgerAccountId ==
                dto.LedgerAccountId);

        if (ledgerCashExists)
            throw new Exception(
                "A Cash Account already exists for this Ledger Account.");

        // ---------------------------------------------------------
        // Create
        // ---------------------------------------------------------

        var entity = new CashAccount
        {
            LedgerAccountId = dto.LedgerAccountId,
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description?.Trim(),
            OpeningBalance = dto.OpeningBalance,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.CashAccounts.Add(entity);

        await _context.SaveChangesAsync();

        dto.Id = entity.Id;

        return dto;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    public async Task<bool> UpdateAsync(
        int id,
        CashAccountDto dto)
    {
        var entity = await _context.CashAccounts
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        dto.Code = dto.Code.Trim().ToUpper();
        dto.Name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new Exception(
                "Code is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new Exception(
                "Name is required.");

        if (dto.LedgerAccountId <= 0)
            throw new Exception(
                "Valid LedgerAccountId is required.");

        if (dto.OpeningBalance < 0)
            throw new Exception(
                "OpeningBalance cannot be negative.");

        // ---------------------------------------------------------
        // Ledger Account
        // ---------------------------------------------------------

        var ledgerAccount = await _context.LedgerAccounts
            .FirstOrDefaultAsync(x =>
                x.Id == dto.LedgerAccountId &&
                x.IsActive);

        if (ledgerAccount == null)
            throw new Exception(
                "Ledger Account not found.");

        // ---------------------------------------------------------
        // Code duplicate
        // ---------------------------------------------------------

        var codeExists = await _context.CashAccounts
            .AnyAsync(x =>
                x.Code == dto.Code &&
                x.Id != id);

        if (codeExists)
            throw new Exception(
                "Cash Account code already exists.");

        // ---------------------------------------------------------
        // Name duplicate
        // ---------------------------------------------------------

        var nameExists = await _context.CashAccounts
            .AnyAsync(x =>
                x.Name == dto.Name &&
                x.Id != id);

        if (nameExists)
            throw new Exception(
                "Cash Account name already exists.");

        // ---------------------------------------------------------
        // Another cash account for ledger
        // ---------------------------------------------------------

        var ledgerCashExists = await _context.CashAccounts
            .AnyAsync(x =>
                x.LedgerAccountId ==
                dto.LedgerAccountId &&
                x.Id != id);

        if (ledgerCashExists)
            throw new Exception(
                "A Cash Account already exists for this Ledger Account.");

        // ---------------------------------------------------------
        // Update
        // ---------------------------------------------------------

        entity.LedgerAccountId = dto.LedgerAccountId;
        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.Description = dto.Description?.Trim();
        entity.OpeningBalance = dto.OpeningBalance;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }


    // =========================================================
    // DELETE
    // =========================================================

    public async Task<bool> DeleteAsync(
        int id)
    {
        var entity = await _context.CashAccounts
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}