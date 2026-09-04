using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class BankAccountService : IBankAccountService
{
    private readonly TenantDbContext _context;

    public BankAccountService(
        TenantDbContext context)
    {
        _context = context;
    }


    // =========================================================
    // GET ALL
    // =========================================================

    public async Task<List<BankAccountDto>> GetAllAsync()
    {
        return await _context.BankAccounts
            .AsNoTracking()
            .OrderBy(x => x.BankName)
            .ThenBy(x => x.AccountName)
            .Select(x => new BankAccountDto
            {
                Id = x.Id,
                LedgerAccountId = x.LedgerAccountId,
                BankName = x.BankName,
                BranchName = x.BranchName,
                AccountName = x.AccountName,
                AccountNumber = x.AccountNumber,
                AccountType = x.AccountType,
                CurrencyCode = x.CurrencyCode,
                OpeningBalance = x.OpeningBalance,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }


    // =========================================================
    // GET BY ID
    // =========================================================

    public async Task<BankAccountDto?> GetByIdAsync(
        int id)
    {
        return await _context.BankAccounts
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new BankAccountDto
            {
                Id = x.Id,
                LedgerAccountId = x.LedgerAccountId,
                BankName = x.BankName,
                BranchName = x.BranchName,
                AccountName = x.AccountName,
                AccountNumber = x.AccountNumber,
                AccountType = x.AccountType,
                CurrencyCode = x.CurrencyCode,
                OpeningBalance = x.OpeningBalance,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }


    // =========================================================
    // GET BY ACCOUNT NUMBER
    // =========================================================

    public async Task<BankAccountDto?> GetByAccountNumberAsync(
        string accountNumber)
    {
        accountNumber = accountNumber.Trim();

        return await _context.BankAccounts
            .AsNoTracking()
            .Where(x =>
                x.AccountNumber == accountNumber)
            .Select(x => new BankAccountDto
            {
                Id = x.Id,
                LedgerAccountId = x.LedgerAccountId,
                BankName = x.BankName,
                BranchName = x.BranchName,
                AccountName = x.AccountName,
                AccountNumber = x.AccountNumber,
                AccountType = x.AccountType,
                CurrencyCode = x.CurrencyCode,
                OpeningBalance = x.OpeningBalance,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }


    // =========================================================
    // CREATE
    // =========================================================

    public async Task<BankAccountDto> CreateAsync(
        BankAccountDto dto)
    {
        dto.BankName = dto.BankName.Trim();
        dto.BranchName = dto.BranchName?.Trim();
        dto.AccountName = dto.AccountName.Trim();
        dto.AccountNumber = dto.AccountNumber.Trim();
        dto.AccountType =
            string.IsNullOrWhiteSpace(dto.AccountType)
                ? "Current"
                : dto.AccountType.Trim();

        dto.CurrencyCode =
            string.IsNullOrWhiteSpace(dto.CurrencyCode)
                ? "NPR"
                : dto.CurrencyCode.Trim().ToUpper();

        if (dto.LedgerAccountId <= 0)
            throw new Exception(
                "Valid LedgerAccountId is required.");

        if (string.IsNullOrWhiteSpace(dto.BankName))
            throw new Exception(
                "BankName is required.");

        if (string.IsNullOrWhiteSpace(dto.AccountName))
            throw new Exception(
                "AccountName is required.");

        if (string.IsNullOrWhiteSpace(dto.AccountNumber))
            throw new Exception(
                "AccountNumber is required.");

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
        // Account Number duplicate
        // ---------------------------------------------------------

        var accountExists = await _context.BankAccounts
            .AnyAsync(x =>
                x.AccountNumber ==
                dto.AccountNumber);

        if (accountExists)
            throw new Exception(
                "Bank Account Number already exists.");

        // ---------------------------------------------------------
        // Create
        // ---------------------------------------------------------

        var entity = new BankAccount
        {
            LedgerAccountId = dto.LedgerAccountId,
            BankName = dto.BankName,
            BranchName = dto.BranchName,
            AccountName = dto.AccountName,
            AccountNumber = dto.AccountNumber,
            AccountType = dto.AccountType,
            CurrencyCode = dto.CurrencyCode,
            OpeningBalance = dto.OpeningBalance,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.BankAccounts.Add(entity);

        await _context.SaveChangesAsync();

        dto.Id = entity.Id;

        return dto;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    public async Task<bool> UpdateAsync(
        int id,
        BankAccountDto dto)
    {
        var entity = await _context.BankAccounts
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        dto.BankName = dto.BankName.Trim();
        dto.BranchName = dto.BranchName?.Trim();
        dto.AccountName = dto.AccountName.Trim();
        dto.AccountNumber = dto.AccountNumber.Trim();

        dto.AccountType =
            string.IsNullOrWhiteSpace(dto.AccountType)
                ? "Current"
                : dto.AccountType.Trim();

        dto.CurrencyCode =
            string.IsNullOrWhiteSpace(dto.CurrencyCode)
                ? "NPR"
                : dto.CurrencyCode.Trim().ToUpper();

        if (dto.LedgerAccountId <= 0)
            throw new Exception(
                "Valid LedgerAccountId is required.");

        if (string.IsNullOrWhiteSpace(dto.BankName))
            throw new Exception(
                "BankName is required.");

        if (string.IsNullOrWhiteSpace(dto.AccountName))
            throw new Exception(
                "AccountName is required.");

        if (string.IsNullOrWhiteSpace(dto.AccountNumber))
            throw new Exception(
                "AccountNumber is required.");

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
        // Account Number duplicate
        // ---------------------------------------------------------

        var accountExists = await _context.BankAccounts
            .AnyAsync(x =>
                x.AccountNumber ==
                dto.AccountNumber &&
                x.Id != id);

        if (accountExists)
            throw new Exception(
                "Bank Account Number already exists.");

        // ---------------------------------------------------------
        // Update
        // ---------------------------------------------------------

        entity.LedgerAccountId = dto.LedgerAccountId;
        entity.BankName = dto.BankName;
        entity.BranchName = dto.BranchName;
        entity.AccountName = dto.AccountName;
        entity.AccountNumber = dto.AccountNumber;
        entity.AccountType = dto.AccountType;
        entity.CurrencyCode = dto.CurrencyCode;
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
        var entity = await _context.BankAccounts
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}