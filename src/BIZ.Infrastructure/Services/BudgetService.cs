using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class BudgetService : IBudgetService
{
    private readonly TenantDbContext _context;

    public BudgetService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<List<BudgetDto>> GetAllAsync()
    {
        return await _context.Budgets
            .Where(x => x.IsActive)
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new BudgetDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                CostCenterId = x.CostCenterId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                TotalAmount = x.TotalAmount,
                IsApproved = x.IsApproved,
                ApprovedAt = x.ApprovedAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<BudgetDto?> GetByIdAsync(int id)
    {
        return await _context.Budgets
            .Where(x => x.Id == id && x.IsActive)
            .AsNoTracking()
            .Select(x => new BudgetDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                CostCenterId = x.CostCenterId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                TotalAmount = x.TotalAmount,
                IsApproved = x.IsApproved,
                ApprovedAt = x.ApprovedAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<BudgetDto?> GetByCodeAsync(string code)
    {
        code = code.Trim().ToUpperInvariant();

        return await _context.Budgets
            .Where(x => x.Code == code && x.IsActive)
            .AsNoTracking()
            .Select(x => new BudgetDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                CostCenterId = x.CostCenterId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                TotalAmount = x.TotalAmount,
                IsApproved = x.IsApproved,
                ApprovedAt = x.ApprovedAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<BudgetDto>> GetByFiscalYearAsync(int fiscalYearId)
    {
        return await _context.Budgets
            .Where(x => x.FiscalYearId == fiscalYearId && x.IsActive)
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new BudgetDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                CostCenterId = x.CostCenterId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                TotalAmount = x.TotalAmount,
                IsApproved = x.IsApproved,
                ApprovedAt = x.ApprovedAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<BudgetDto> CreateAsync(BudgetDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new InvalidOperationException("Budget code is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("Budget name is required.");

        if (dto.TotalAmount < 0)
            throw new InvalidOperationException("Total amount cannot be negative.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new InvalidOperationException("Fiscal year not found.");

        if (fiscalYear.IsClosed)
            throw new InvalidOperationException("Fiscal year is closed.");

        var code = dto.Code.Trim().ToUpperInvariant();

        var exists = await _context.Budgets
            .AnyAsync(x => x.Code == code);

        if (exists)
            throw new InvalidOperationException(
                "Budget code already exists.");

        if (dto.CostCenterId.HasValue)
        {
            var costCenter = await _context.CostCenters
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.CostCenterId.Value &&
                    x.IsActive);

            if (costCenter == null)
                throw new InvalidOperationException(
                    "Cost center not found.");
        }

        var budget = new Budget
        {
            FiscalYearId = dto.FiscalYearId,
            CostCenterId = dto.CostCenterId,
            Code = code,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            TotalAmount = 0,
            IsApproved = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Budgets.Add(budget);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(budget.Id))!;
    }

    public async Task<bool> UpdateAsync(int id, BudgetDto dto)
    {
        var budget = await _context.Budgets
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (budget == null)
            return false;

        if (budget.IsApproved)
            throw new InvalidOperationException(
                "Approved budget cannot be updated.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == budget.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new InvalidOperationException(
                "Fiscal year not found.");

        if (fiscalYear.IsClosed)
            throw new InvalidOperationException(
                "Fiscal year is closed.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException(
                "Budget name is required.");

        if (dto.CostCenterId.HasValue)
        {
            var costCenter = await _context.CostCenters
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.CostCenterId.Value &&
                    x.IsActive);

            if (costCenter == null)
                throw new InvalidOperationException(
                    "Cost center not found.");
        }

        budget.Name = dto.Name.Trim();
        budget.Description = dto.Description?.Trim();
        budget.CostCenterId = dto.CostCenterId;
        budget.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ApproveAsync(int id)
    {
        var budget = await _context.Budgets
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (budget == null)
            return false;

        if (budget.IsApproved)
            throw new InvalidOperationException(
                "Budget is already approved.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == budget.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new InvalidOperationException(
                "Fiscal year not found.");

        if (fiscalYear.IsClosed)
            throw new InvalidOperationException(
                "Fiscal year is closed.");

        var lineCount = await _context.BudgetLines
            .CountAsync(x => x.BudgetId == id);

        if (lineCount == 0)
            throw new InvalidOperationException(
                "Budget must have at least one line before approval.");

        var total = await _context.BudgetLines
            .Where(x => x.BudgetId == id)
            .SumAsync(x => x.BudgetAmount);

        if (total <= 0)
            throw new InvalidOperationException(
                "Budget amount must be greater than zero.");

        budget.TotalAmount = total;
        budget.IsApproved = true;
        budget.ApprovedAt = DateTime.UtcNow;
        budget.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var budget = await _context.Budgets
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (budget == null)
            return false;

        if (budget.IsApproved)
            throw new InvalidOperationException(
                "Approved budget cannot be deleted.");

        budget.IsActive = false;
        budget.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}