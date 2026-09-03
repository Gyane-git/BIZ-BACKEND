using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class SubLedgerService : ISubLedgerService
{
    private readonly TenantDbContext _context;

    public SubLedgerService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<List<SubLedgerDto>> GetAllAsync()
    {
        return await _context.SubLedgers
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new SubLedgerDto
            {
                Id = x.Id,
                LedgerAccountId = x.LedgerAccountId,
                Code = x.Code,
                Name = x.Name,
                ContactPerson = x.ContactPerson,
                Phone = x.Phone,
                Email = x.Email,
                Address = x.Address,
                TaxNumber = x.TaxNumber,
                OpeningDebit = x.OpeningDebit,
                OpeningCredit = x.OpeningCredit,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<SubLedgerDto>> GetByLedgerAccountAsync(
        int ledgerAccountId)
    {
        return await _context.SubLedgers
            .AsNoTracking()
            .Where(x =>
                x.LedgerAccountId == ledgerAccountId &&
                x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new SubLedgerDto
            {
                Id = x.Id,
                LedgerAccountId = x.LedgerAccountId,
                Code = x.Code,
                Name = x.Name,
                ContactPerson = x.ContactPerson,
                Phone = x.Phone,
                Email = x.Email,
                Address = x.Address,
                TaxNumber = x.TaxNumber,
                OpeningDebit = x.OpeningDebit,
                OpeningCredit = x.OpeningCredit,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<SubLedgerDto?> GetByIdAsync(int id)
    {
        return await _context.SubLedgers
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new SubLedgerDto
            {
                Id = x.Id,
                LedgerAccountId = x.LedgerAccountId,
                Code = x.Code,
                Name = x.Name,
                ContactPerson = x.ContactPerson,
                Phone = x.Phone,
                Email = x.Email,
                Address = x.Address,
                TaxNumber = x.TaxNumber,
                OpeningDebit = x.OpeningDebit,
                OpeningCredit = x.OpeningCredit,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SubLedgerDto?> GetByCodeAsync(string code)
    {
        code = code.Trim().ToUpper();

        return await _context.SubLedgers
            .AsNoTracking()
            .Where(x =>
                x.Code == code &&
                x.IsActive)
            .Select(x => new SubLedgerDto
            {
                Id = x.Id,
                LedgerAccountId = x.LedgerAccountId,
                Code = x.Code,
                Name = x.Name,
                ContactPerson = x.ContactPerson,
                Phone = x.Phone,
                Email = x.Email,
                Address = x.Address,
                TaxNumber = x.TaxNumber,
                OpeningDebit = x.OpeningDebit,
                OpeningCredit = x.OpeningCredit,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SubLedgerDto> CreateAsync(SubLedgerDto dto)
    {
        var code = dto.Code.Trim().ToUpper();
        var name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("SubLedger code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("SubLedger name is required.");

        if (dto.OpeningDebit < 0)
            throw new ArgumentException(
                "Opening debit cannot be negative.");

        if (dto.OpeningCredit < 0)
            throw new ArgumentException(
                "Opening credit cannot be negative.");

        if (dto.OpeningDebit > 0 && dto.OpeningCredit > 0)
            throw new ArgumentException(
                "Opening debit and opening credit cannot both be greater than zero.");

        var ledgerAccount = await _context.LedgerAccounts
            .FirstOrDefaultAsync(x =>
                x.Id == dto.LedgerAccountId &&
                x.IsActive);

        if (ledgerAccount == null)
            throw new ArgumentException(
                "Active LedgerAccount not found.");

        var codeExists = await _context.SubLedgers
            .AnyAsync(x => x.Code == code);

        if (codeExists)
            throw new InvalidOperationException(
                $"SubLedger code '{code}' already exists.");

        var nameExists = await _context.SubLedgers
            .AnyAsync(x =>
                x.LedgerAccountId == dto.LedgerAccountId &&
                x.Name == name);

        if (nameExists)
            throw new InvalidOperationException(
                $"SubLedger name '{name}' already exists under this LedgerAccount.");

        var entity = new SubLedger
        {
            LedgerAccountId = dto.LedgerAccountId,
            Code = code,
            Name = name,
            ContactPerson = dto.ContactPerson?.Trim(),
            Phone = dto.Phone?.Trim(),
            Email = dto.Email?.Trim(),
            Address = dto.Address?.Trim(),
            TaxNumber = dto.TaxNumber?.Trim(),
            OpeningDebit = dto.OpeningDebit,
            OpeningCredit = dto.OpeningCredit,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.SubLedgers.Add(entity);

        await _context.SaveChangesAsync();

        dto.Id = entity.Id;
        dto.Code = entity.Code;
        dto.Name = entity.Name;
        dto.IsActive = entity.IsActive;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        SubLedgerDto dto)
    {
        var entity = await _context.SubLedgers
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (entity == null)
            return false;

        var code = dto.Code.Trim().ToUpper();
        var name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("SubLedger code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("SubLedger name is required.");

        if (dto.OpeningDebit < 0)
            throw new ArgumentException(
                "Opening debit cannot be negative.");

        if (dto.OpeningCredit < 0)
            throw new ArgumentException(
                "Opening credit cannot be negative.");

        if (dto.OpeningDebit > 0 && dto.OpeningCredit > 0)
            throw new ArgumentException(
                "Opening debit and opening credit cannot both be greater than zero.");

        var ledgerAccount = await _context.LedgerAccounts
            .FirstOrDefaultAsync(x =>
                x.Id == dto.LedgerAccountId &&
                x.IsActive);

        if (ledgerAccount == null)
            throw new ArgumentException(
                "Active LedgerAccount not found.");

        var codeExists = await _context.SubLedgers
            .AnyAsync(x =>
                x.Id != id &&
                x.Code == code);

        if (codeExists)
            throw new InvalidOperationException(
                $"SubLedger code '{code}' already exists.");

        var nameExists = await _context.SubLedgers
            .AnyAsync(x =>
                x.Id != id &&
                x.LedgerAccountId == dto.LedgerAccountId &&
                x.Name == name);

        if (nameExists)
            throw new InvalidOperationException(
                $"SubLedger name '{name}' already exists under this LedgerAccount.");

        entity.LedgerAccountId = dto.LedgerAccountId;
        entity.Code = code;
        entity.Name = name;
        entity.ContactPerson = dto.ContactPerson?.Trim();
        entity.Phone = dto.Phone?.Trim();
        entity.Email = dto.Email?.Trim();
        entity.Address = dto.Address?.Trim();
        entity.TaxNumber = dto.TaxNumber?.Trim();
        entity.OpeningDebit = dto.OpeningDebit;
        entity.OpeningCredit = dto.OpeningCredit;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.SubLedgers
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (entity == null)
            return false;

        // Soft delete
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}