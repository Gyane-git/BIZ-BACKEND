using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class StockAdjustmentService : IStockAdjustmentService
{
    private readonly TenantDbContext _context;

    public StockAdjustmentService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StockAdjustmentDto>> GetAllAsync()
    {
        return await _context.StockAdjustments
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.AdjustmentDate)
            .ThenByDescending(x => x.Id)
            .Select(x => new StockAdjustmentDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                WarehouseId = x.WarehouseId,
                BranchId = x.BranchId,
                AdjustmentNumber = x.AdjustmentNumber,
                AdjustmentDate = x.AdjustmentDate,
                AdjustmentType = x.AdjustmentType,
                ReferenceNumber = x.ReferenceNumber,
                Reason = x.Reason,
                TotalIncreaseQuantity = x.TotalIncreaseQuantity,
                TotalDecreaseQuantity = x.TotalDecreaseQuantity,
                TotalValue = x.TotalValue,
                Status = x.Status,
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<StockAdjustmentDto?> GetByIdAsync(int id)
    {
        var adjustment = await _context.StockAdjustments
            .AsNoTracking()
            .Include(x => x.StockAdjustmentLines)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (adjustment == null)
            return null;

        return MapToDto(adjustment);
    }

    public async Task<StockAdjustmentDto> CreateAsync(
        StockAdjustmentDto dto)
    {
        ValidateHeader(dto);

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new ArgumentException(
                "Fiscal year not found or inactive.");

        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearPeriodId &&
                x.IsActive);

        if (period == null)
            throw new ArgumentException(
                "Fiscal year period not found or inactive.");

        if (period.FiscalYearId != dto.FiscalYearId)
            throw new ArgumentException(
                "Fiscal year period does not belong to the selected fiscal year.");

        if (dto.AdjustmentDate.Date < period.StartDate.Date ||
            dto.AdjustmentDate.Date > period.EndDate.Date)
        {
            throw new ArgumentException(
                "Adjustment date must be within the fiscal year period.");
        }

        var duplicate = await _context.StockAdjustments
            .AnyAsync(x =>
                x.AdjustmentNumber == dto.AdjustmentNumber &&
                x.IsActive);

        if (duplicate)
            throw new ArgumentException(
                "Adjustment number already exists.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException(
                "At least one adjustment line is required.");

        var adjustment = new StockAdjustment
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,
            WarehouseId = dto.WarehouseId,
            BranchId = dto.BranchId,
            AdjustmentNumber = dto.AdjustmentNumber.Trim().ToUpperInvariant(),
            AdjustmentDate = dto.AdjustmentDate,
            AdjustmentType = dto.AdjustmentType.Trim(),
            ReferenceNumber = dto.ReferenceNumber?.Trim(),
            Reason = dto.Reason?.Trim(),
            Status = "Draft",
            IsPosted = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        decimal totalIncrease = 0;
        decimal totalDecrease = 0;
        decimal totalValue = 0;

        foreach (var lineDto in dto.Lines.OrderBy(x => x.LineNumber))
        {
            if (lineDto.ProductId <= 0)
                throw new ArgumentException(
                    "ProductId must be greater than zero.");

            if (lineDto.Quantity <= 0)
                throw new ArgumentException(
                    "Line quantity must be greater than zero.");

            if (lineDto.UnitCost < 0)
                throw new ArgumentException(
                    "UnitCost cannot be negative.");

            var product = await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == lineDto.ProductId &&
                    x.IsActive);

            if (product == null)
                throw new ArgumentException(
                    $"Product {lineDto.ProductId} not found or inactive.");

            var adjustmentType =
                lineDto.AdjustmentType.Trim();

            if (adjustmentType != "Increase" &&
                adjustmentType != "Decrease")
            {
                throw new ArgumentException(
                    "AdjustmentType must be Increase or Decrease.");
            }

            var lineTotal =
                lineDto.Quantity * lineDto.UnitCost;

            var line = new StockAdjustmentLine
            {
                ProductId = lineDto.ProductId,
                Quantity = lineDto.Quantity,
                UnitCost = lineDto.UnitCost,
                LineTotal = lineTotal,
                AdjustmentType = adjustmentType,
                Description = lineDto.Description?.Trim(),
                LineNumber = lineDto.LineNumber
            };

            adjustment.StockAdjustmentLines.Add(line);

            if (adjustmentType == "Increase")
                totalIncrease += lineDto.Quantity;
            else
                totalDecrease += lineDto.Quantity;

            totalValue += lineTotal;
        }

        adjustment.TotalIncreaseQuantity = totalIncrease;
        adjustment.TotalDecreaseQuantity = totalDecrease;
        adjustment.TotalValue = totalValue;

        _context.StockAdjustments.Add(adjustment);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(adjustment.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        StockAdjustmentDto dto)
    {
        var adjustment = await _context.StockAdjustments
            .Include(x => x.StockAdjustmentLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (adjustment == null)
            return false;

        if (adjustment.IsPosted)
            throw new InvalidOperationException(
                "Posted stock adjustment cannot be updated.");

        ValidateHeader(dto);

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException(
                "At least one adjustment line is required.");

        adjustment.FiscalYearId = dto.FiscalYearId;
        adjustment.FiscalYearPeriodId = dto.FiscalYearPeriodId;
        adjustment.WarehouseId = dto.WarehouseId;
        adjustment.BranchId = dto.BranchId;
        adjustment.AdjustmentNumber =
            dto.AdjustmentNumber.Trim().ToUpperInvariant();
        adjustment.AdjustmentDate = dto.AdjustmentDate;
        adjustment.AdjustmentType = dto.AdjustmentType.Trim();
        adjustment.ReferenceNumber = dto.ReferenceNumber?.Trim();
        adjustment.Reason = dto.Reason?.Trim();

        _context.StockAdjustmentLines.RemoveRange(
            adjustment.StockAdjustmentLines);

        decimal totalIncrease = 0;
        decimal totalDecrease = 0;
        decimal totalValue = 0;

        foreach (var lineDto in dto.Lines.OrderBy(x => x.LineNumber))
        {
            if (lineDto.ProductId <= 0)
                throw new ArgumentException(
                    "ProductId must be greater than zero.");

            if (lineDto.Quantity <= 0)
                throw new ArgumentException(
                    "Line quantity must be greater than zero.");

            if (lineDto.UnitCost < 0)
                throw new ArgumentException(
                    "UnitCost cannot be negative.");

            var product = await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == lineDto.ProductId &&
                    x.IsActive);

            if (product == null)
                throw new ArgumentException(
                    $"Product {lineDto.ProductId} not found or inactive.");

            var adjustmentType =
                lineDto.AdjustmentType.Trim();

            if (adjustmentType != "Increase" &&
                adjustmentType != "Decrease")
            {
                throw new ArgumentException(
                    "AdjustmentType must be Increase or Decrease.");
            }

            var lineTotal =
                lineDto.Quantity * lineDto.UnitCost;

            adjustment.StockAdjustmentLines.Add(
                new StockAdjustmentLine
                {
                    ProductId = lineDto.ProductId,
                    Quantity = lineDto.Quantity,
                    UnitCost = lineDto.UnitCost,
                    LineTotal = lineTotal,
                    AdjustmentType = adjustmentType,
                    Description = lineDto.Description?.Trim(),
                    LineNumber = lineDto.LineNumber
                });

            if (adjustmentType == "Increase")
                totalIncrease += lineDto.Quantity;
            else
                totalDecrease += lineDto.Quantity;

            totalValue += lineTotal;
        }

        adjustment.TotalIncreaseQuantity = totalIncrease;
        adjustment.TotalDecreaseQuantity = totalDecrease;
        adjustment.TotalValue = totalValue;
        adjustment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var adjustment = await _context.StockAdjustments
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (adjustment == null)
            return false;

        if (adjustment.IsPosted)
            throw new InvalidOperationException(
                "Posted stock adjustment cannot be deleted.");

        adjustment.IsActive = false;
        adjustment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PostAsync(int id)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var adjustment = await _context.StockAdjustments
                .Include(x => x.StockAdjustmentLines)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsActive);

            if (adjustment == null)
                return false;

            if (adjustment.IsPosted)
                throw new InvalidOperationException(
                    "Stock adjustment is already posted.");

            if (adjustment.StockAdjustmentLines.Count == 0)
                throw new InvalidOperationException(
                    "Stock adjustment has no lines.");

            foreach (var line in adjustment.StockAdjustmentLines
                         .OrderBy(x => x.LineNumber))
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(x =>
                        x.Id == line.ProductId &&
                        x.IsActive);

                if (product == null)
                    throw new InvalidOperationException(
                        $"Product {line.ProductId} not found or inactive.");

                var balance = await _context.StockBalances
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == line.ProductId &&
                        x.WarehouseId == adjustment.WarehouseId &&
                        x.BranchId == adjustment.BranchId &&
                        x.IsActive);

                if (balance == null)
                {
                    if (line.AdjustmentType == "Decrease")
                        throw new InvalidOperationException(
                            $"No stock balance exists for Product {line.ProductId}.");

                    balance = new StockBalance
                    {
                        ProductId = line.ProductId,
                        WarehouseId = adjustment.WarehouseId,
                        BranchId = adjustment.BranchId,
                        Quantity = 0,
                        ReservedQuantity = 0,
                        AvailableQuantity = 0,
                        AverageCost = line.UnitCost,
                        StockValue = 0,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.StockBalances.Add(balance);
                    await _context.SaveChangesAsync();
                }

                decimal transactionQuantityIn = 0;
                decimal transactionQuantityOut = 0;
                decimal transactionUnitCost;

                if (line.AdjustmentType == "Increase")
                {
                    transactionQuantityIn = line.Quantity;

                    transactionUnitCost = line.UnitCost;

                    var oldQuantity = balance.Quantity;
                    var oldAverageCost = balance.AverageCost;

                    var newQuantity =
                        oldQuantity + line.Quantity;

                    if (newQuantity > 0)
                    {
                        balance.AverageCost =
                            ((oldQuantity * oldAverageCost) +
                             (line.Quantity * line.UnitCost))
                            / newQuantity;
                    }

                    balance.Quantity = newQuantity;
                }
                else
                {
                    if (line.Quantity > balance.AvailableQuantity)
                    {
                        throw new InvalidOperationException(
                            $"Insufficient available stock for Product {line.ProductId}.");
                    }

                    transactionQuantityOut = line.Quantity;

                    transactionUnitCost =
                        balance.AverageCost;

                    balance.Quantity -= line.Quantity;
                }

                balance.AvailableQuantity =
                    balance.Quantity -
                    balance.ReservedQuantity;

                balance.StockValue =
                    balance.Quantity *
                    balance.AverageCost;

                var stockTransaction = new StockTransaction
                {
                    ProductId = line.ProductId,
                    WarehouseId = adjustment.WarehouseId,
                    BranchId = adjustment.BranchId,
                    FiscalYearId = adjustment.FiscalYearId,
                    FiscalYearPeriodId = adjustment.FiscalYearPeriodId,
                    TransactionDate = adjustment.AdjustmentDate,
                    TransactionType =
                        line.AdjustmentType == "Increase"
                            ? "AdjustmentIn"
                            : "AdjustmentOut",
                    ReferenceType = "StockAdjustment",
                    ReferenceId = adjustment.Id,
                    ReferenceNumber =
                        adjustment.AdjustmentNumber,
                    QuantityIn = transactionQuantityIn,
                    QuantityOut = transactionQuantityOut,
                    BalanceQuantity = balance.Quantity,
                    UnitCost = transactionUnitCost,
                    TotalCost =
                        line.Quantity * transactionUnitCost,
                    Description = line.Description ??
                                  adjustment.Reason,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.StockTransactions.Add(stockTransaction);
            }

            adjustment.Status = "Posted";
            adjustment.IsPosted = true;
            adjustment.PostedAt = DateTime.UtcNow;
            adjustment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static void ValidateHeader(
        StockAdjustmentDto dto)
    {
        if (dto.FiscalYearId <= 0)
            throw new ArgumentException(
                "FiscalYearId must be greater than zero.");

        if (dto.FiscalYearPeriodId <= 0)
            throw new ArgumentException(
                "FiscalYearPeriodId must be greater than zero.");

        if (string.IsNullOrWhiteSpace(dto.AdjustmentNumber))
            throw new ArgumentException(
                "AdjustmentNumber is required.");

        if (dto.AdjustmentDate == default)
            throw new ArgumentException(
                "AdjustmentDate is required.");

        var adjustmentType =
            dto.AdjustmentType.Trim();

        if (adjustmentType != "Increase" &&
            adjustmentType != "Decrease" &&
            adjustmentType != "Mixed")
        {
            throw new ArgumentException(
                "AdjustmentType must be Increase, Decrease or Mixed.");
        }
    }

    private static StockAdjustmentDto MapToDto(
        StockAdjustment adjustment)
    {
        return new StockAdjustmentDto
        {
            Id = adjustment.Id,
            FiscalYearId = adjustment.FiscalYearId,
            FiscalYearPeriodId = adjustment.FiscalYearPeriodId,
            WarehouseId = adjustment.WarehouseId,
            BranchId = adjustment.BranchId,
            AdjustmentNumber = adjustment.AdjustmentNumber,
            AdjustmentDate = adjustment.AdjustmentDate,
            AdjustmentType = adjustment.AdjustmentType,
            ReferenceNumber = adjustment.ReferenceNumber,
            Reason = adjustment.Reason,
            TotalIncreaseQuantity =
                adjustment.TotalIncreaseQuantity,
            TotalDecreaseQuantity =
                adjustment.TotalDecreaseQuantity,
            TotalValue = adjustment.TotalValue,
            Status = adjustment.Status,
            IsPosted = adjustment.IsPosted,
            PostedAt = adjustment.PostedAt,
            IsActive = adjustment.IsActive,
            CreatedAt = adjustment.CreatedAt,
            UpdatedAt = adjustment.UpdatedAt,
            Lines = adjustment.StockAdjustmentLines
                .OrderBy(x => x.LineNumber)
                .Select(x => new StockAdjustmentLineDto
                {
                    Id = x.Id,
                    StockAdjustmentId = x.StockAdjustmentId,
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    UnitCost = x.UnitCost,
                    LineTotal = x.LineTotal,
                    AdjustmentType = x.AdjustmentType,
                    Description = x.Description,
                    LineNumber = x.LineNumber
                })
                .ToList()
        };
    }
}