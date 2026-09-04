using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class BudgetLineService : IBudgetLineService
{
    private readonly TenantDbContext _context;

    public BudgetLineService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<List<BudgetLineDto>> GetAllAsync()
    {
        return await _context.BudgetLines
            .AsNoTracking()
            .OrderBy(x => x.BudgetId)
            .ThenBy(x => x.LineNumber)
            .Select(x => new BudgetLineDto
            {
                Id = x.Id,
                BudgetId = x.BudgetId,
                LedgerAccountId = x.LedgerAccountId,
                CostCenterId = x.CostCenterId,
                BudgetAmount = x.BudgetAmount,
                RevisedAmount = x.RevisedAmount,
                Description = x.Description,
                LineNumber = x.LineNumber
            })
            .ToListAsync();
    }

    public async Task<List<BudgetLineDto>> GetByBudgetAsync(int budgetId)
    {
        return await _context.BudgetLines
            .Where(x => x.BudgetId == budgetId)
            .AsNoTracking()
            .OrderBy(x => x.LineNumber)
            .Select(x => new BudgetLineDto
            {
                Id = x.Id,
                BudgetId = x.BudgetId,
                LedgerAccountId = x.LedgerAccountId,
                CostCenterId = x.CostCenterId,
                BudgetAmount = x.BudgetAmount,
                RevisedAmount = x.RevisedAmount,
                Description = x.Description,
                LineNumber = x.LineNumber
            })
            .ToListAsync();
    }

    public async Task<BudgetLineDto?> GetByIdAsync(int id)
    {
        return await _context.BudgetLines
            .Where(x => x.Id == id)
            .AsNoTracking()
            .Select(x => new BudgetLineDto
            {
                Id = x.Id,
                BudgetId = x.BudgetId,
                LedgerAccountId = x.LedgerAccountId,
                CostCenterId = x.CostCenterId,
                BudgetAmount = x.BudgetAmount,
                RevisedAmount = x.RevisedAmount,
                Description = x.Description,
                LineNumber = x.LineNumber
            })
            .FirstOrDefaultAsync();
    }

    public async Task<BudgetLineDto> CreateAsync(BudgetLineDto dto)
    {
        if (dto.BudgetAmount < 0)
            throw new InvalidOperationException(
                "Budget amount cannot be negative.");

        if (dto.RevisedAmount < 0)
            throw new InvalidOperationException(
                "Revised amount cannot be negative.");

        if (dto.LineNumber <= 0)
            throw new InvalidOperationException(
                "Line number must be greater than zero.");

        var budget = await _context.Budgets
            .FirstOrDefaultAsync(x =>
                x.Id == dto.BudgetId &&
                x.IsActive);

        if (budget == null)
            throw new InvalidOperationException(
                "Budget not found.");

        if (budget.IsApproved)
            throw new InvalidOperationException(
                "Approved budget cannot be modified.");

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

        var ledger = await _context.LedgerAccounts
            .FirstOrDefaultAsync(x =>
                x.Id == dto.LedgerAccountId &&
                x.IsActive);

        if (ledger == null)
            throw new InvalidOperationException(
                "Ledger account not found.");

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

        var lineExists = await _context.BudgetLines
            .AnyAsync(x =>
                x.BudgetId == dto.BudgetId &&
                x.LineNumber == dto.LineNumber);

        if (lineExists)
            throw new InvalidOperationException(
                "Line number already exists for this budget.");

        var line = new BudgetLine
        {
            BudgetId = dto.BudgetId,
            LedgerAccountId = dto.LedgerAccountId,
            CostCenterId = dto.CostCenterId,
            BudgetAmount = dto.BudgetAmount,
            RevisedAmount = dto.RevisedAmount,
            Description = dto.Description?.Trim(),
            LineNumber = dto.LineNumber
        };

        _context.BudgetLines.Add(line);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(line.Id))!;
    }

    public async Task<bool> UpdateAsync(int id, BudgetLineDto dto)
    {
        var line = await _context.BudgetLines
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        var budget = await _context.Budgets
            .FirstOrDefaultAsync(x =>
                x.Id == line.BudgetId &&
                x.IsActive);

        if (budget == null)
            throw new InvalidOperationException(
                "Budget not found.");

        if (budget.IsApproved)
            throw new InvalidOperationException(
                "Approved budget cannot be modified.");

        if (dto.BudgetAmount < 0 ||
            dto.RevisedAmount < 0)
            throw new InvalidOperationException(
                "Budget amounts cannot be negative.");

        if (dto.LineNumber <= 0)
            throw new InvalidOperationException(
                "Line number must be greater than zero.");

        var ledger = await _context.LedgerAccounts
            .FirstOrDefaultAsync(x =>
                x.Id == dto.LedgerAccountId &&
                x.IsActive);

        if (ledger == null)
            throw new InvalidOperationException(
                "Ledger account not found.");

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

        var duplicate = await _context.BudgetLines
            .AnyAsync(x =>
                x.BudgetId == line.BudgetId &&
                x.LineNumber == dto.LineNumber &&
                x.Id != id);

        if (duplicate)
            throw new InvalidOperationException(
                "Line number already exists for this budget.");

        line.LedgerAccountId = dto.LedgerAccountId;
        line.CostCenterId = dto.CostCenterId;
        line.BudgetAmount = dto.BudgetAmount;
        line.RevisedAmount = dto.RevisedAmount;
        line.Description = dto.Description?.Trim();
        line.LineNumber = dto.LineNumber;

        budget.TotalAmount = await _context.BudgetLines
            .Where(x => x.BudgetId == line.BudgetId &&
                        x.Id != id)
            .SumAsync(x => x.BudgetAmount) + dto.BudgetAmount;

        budget.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var line = await _context.BudgetLines
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        var budget = await _context.Budgets
            .FirstOrDefaultAsync(x =>
                x.Id == line.BudgetId &&
                x.IsActive);

        if (budget == null)
            throw new InvalidOperationException(
                "Budget not found.");

        if (budget.IsApproved)
            throw new InvalidOperationException(
                "Approved budget cannot be modified.");

        _context.BudgetLines.Remove(line);

        budget.TotalAmount = await _context.BudgetLines
            .Where(x =>
                x.BudgetId == line.BudgetId &&
                x.Id != id)
            .SumAsync(x => x.BudgetAmount);

        budget.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}