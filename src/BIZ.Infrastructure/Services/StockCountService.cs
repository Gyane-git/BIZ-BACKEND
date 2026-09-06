using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class StockCountService : IStockCountService
{
    private readonly TenantDbContext _context;

    public StockCountService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StockCountDto>> GetAllAsync()
    {
        return await _context.StockCounts
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CountDate)
            .ThenByDescending(x => x.Id)
            .Select(x => new StockCountDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                WarehouseId = x.WarehouseId,
                BranchId = x.BranchId,
                CountNumber = x.CountNumber,
                CountDate = x.CountDate,
                ReferenceNumber = x.ReferenceNumber,
                Reason = x.Reason,
                TotalSystemQuantity = x.TotalSystemQuantity,
                TotalCountedQuantity = x.TotalCountedQuantity,
                TotalDifferenceQuantity = x.TotalDifferenceQuantity,
                TotalDifferenceValue = x.TotalDifferenceValue,
                Status = x.Status,
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<StockCountDto?> GetByIdAsync(int id)
    {
        var count = await _context.StockCounts
            .AsNoTracking()
            .Include(x => x.StockCountLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (count == null)
            return null;

        return MapToDto(count);
    }

    public async Task<StockCountDto> CreateAsync(
        StockCountDto dto)
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
                "Fiscal year period does not belong to selected fiscal year.");

        if (dto.CountDate.Date < period.StartDate.Date ||
            dto.CountDate.Date > period.EndDate.Date)
        {
            throw new ArgumentException(
                "Count date must be within fiscal year period.");
        }

        var countNumber =
            dto.CountNumber.Trim().ToUpperInvariant();

        var duplicate = await _context.StockCounts
            .AnyAsync(x =>
                x.CountNumber == countNumber &&
                x.IsActive);

        if (duplicate)
            throw new ArgumentException(
                "Count number already exists.");

        if (dto.Lines == null ||
            dto.Lines.Count == 0)
        {
            throw new ArgumentException(
                "At least one stock count line is required.");
        }

        var stockCount = new StockCount
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,

            WarehouseId = dto.WarehouseId,
            BranchId = dto.BranchId,

            CountNumber = countNumber,

            CountDate = dto.CountDate,

            ReferenceNumber =
                dto.ReferenceNumber?.Trim(),

            Reason =
                dto.Reason?.Trim(),

            Status = "Draft",

            IsPosted = false,

            IsActive = true,

            CreatedAt = DateTime.UtcNow
        };

        decimal totalSystem = 0;
        decimal totalCounted = 0;
        decimal totalDifference = 0;
        decimal totalDifferenceValue = 0;

        foreach (var lineDto in dto.Lines
                     .OrderBy(x => x.LineNumber))
        {
            ValidateLine(lineDto);

            var product =
                await _context.Products
                    .FirstOrDefaultAsync(x =>
                        x.Id == lineDto.ProductId &&
                        x.IsActive);

            if (product == null)
                throw new ArgumentException(
                    $"Product {lineDto.ProductId} not found or inactive.");

            var balance =
                await _context.StockBalances
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == lineDto.ProductId &&
                        x.WarehouseId == dto.WarehouseId &&
                        x.BranchId == dto.BranchId &&
                        x.IsActive);

            var systemQuantity =
                balance?.Quantity ?? 0;

            var difference =
                lineDto.CountedQuantity -
                systemQuantity;

            var differenceValue =
                difference *
                lineDto.UnitCost;

            stockCount.StockCountLines.Add(
                new StockCountLine
                {
                    ProductId =
                        lineDto.ProductId,

                    SystemQuantity =
                        systemQuantity,

                    CountedQuantity =
                        lineDto.CountedQuantity,

                    DifferenceQuantity =
                        difference,

                    UnitCost =
                        lineDto.UnitCost,

                    DifferenceValue =
                        differenceValue,

                    Description =
                        lineDto.Description?.Trim(),

                    LineNumber =
                        lineDto.LineNumber
                });

            totalSystem += systemQuantity;
            totalCounted += lineDto.CountedQuantity;
            totalDifference += difference;
            totalDifferenceValue += differenceValue;
        }

        stockCount.TotalSystemQuantity =
            totalSystem;

        stockCount.TotalCountedQuantity =
            totalCounted;

        stockCount.TotalDifferenceQuantity =
            totalDifference;

        stockCount.TotalDifferenceValue =
            totalDifferenceValue;

        _context.StockCounts.Add(stockCount);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(stockCount.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        StockCountDto dto)
    {
        var stockCount =
            await _context.StockCounts
                .Include(x => x.StockCountLines)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsActive);

        if (stockCount == null)
            return false;

        if (stockCount.IsPosted)
            throw new InvalidOperationException(
                "Posted stock count cannot be updated.");

        ValidateHeader(dto);

        if (dto.Lines == null ||
            dto.Lines.Count == 0)
        {
            throw new ArgumentException(
                "At least one stock count line is required.");
        }

        var fiscalYear =
            await _context.FiscalYears
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.FiscalYearId &&
                    x.IsActive);

        if (fiscalYear == null)
            throw new ArgumentException(
                "Fiscal year not found or inactive.");

        var period =
            await _context.FiscalYearPeriods
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.FiscalYearPeriodId &&
                    x.IsActive);

        if (period == null)
            throw new ArgumentException(
                "Fiscal year period not found or inactive.");

        if (period.FiscalYearId != dto.FiscalYearId)
            throw new ArgumentException(
                "Fiscal year period does not belong to selected fiscal year.");

        if (dto.CountDate.Date < period.StartDate.Date ||
            dto.CountDate.Date > period.EndDate.Date)
        {
            throw new ArgumentException(
                "Count date must be within fiscal year period.");
        }

        stockCount.FiscalYearId =
            dto.FiscalYearId;

        stockCount.FiscalYearPeriodId =
            dto.FiscalYearPeriodId;

        stockCount.WarehouseId =
            dto.WarehouseId;

        stockCount.BranchId =
            dto.BranchId;

        stockCount.CountNumber =
            dto.CountNumber.Trim().ToUpperInvariant();

        stockCount.CountDate =
            dto.CountDate;

        stockCount.ReferenceNumber =
            dto.ReferenceNumber?.Trim();

        stockCount.Reason =
            dto.Reason?.Trim();

        _context.StockCountLines.RemoveRange(
            stockCount.StockCountLines);

        decimal totalSystem = 0;
        decimal totalCounted = 0;
        decimal totalDifference = 0;
        decimal totalDifferenceValue = 0;

        foreach (var lineDto in dto.Lines
                     .OrderBy(x => x.LineNumber))
        {
            ValidateLine(lineDto);

            var product =
                await _context.Products
                    .FirstOrDefaultAsync(x =>
                        x.Id == lineDto.ProductId &&
                        x.IsActive);

            if (product == null)
                throw new ArgumentException(
                    $"Product {lineDto.ProductId} not found or inactive.");

            var balance =
                await _context.StockBalances
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == lineDto.ProductId &&
                        x.WarehouseId == dto.WarehouseId &&
                        x.BranchId == dto.BranchId &&
                        x.IsActive);

            var systemQuantity =
                balance?.Quantity ?? 0;

            var difference =
                lineDto.CountedQuantity -
                systemQuantity;

            var differenceValue =
                difference *
                lineDto.UnitCost;

            stockCount.StockCountLines.Add(
                new StockCountLine
                {
                    ProductId =
                        lineDto.ProductId,

                    SystemQuantity =
                        systemQuantity,

                    CountedQuantity =
                        lineDto.CountedQuantity,

                    DifferenceQuantity =
                        difference,

                    UnitCost =
                        lineDto.UnitCost,

                    DifferenceValue =
                        differenceValue,

                    Description =
                        lineDto.Description?.Trim(),

                    LineNumber =
                        lineDto.LineNumber
                });

            totalSystem += systemQuantity;
            totalCounted += lineDto.CountedQuantity;
            totalDifference += difference;
            totalDifferenceValue += differenceValue;
        }

        stockCount.TotalSystemQuantity =
            totalSystem;

        stockCount.TotalCountedQuantity =
            totalCounted;

        stockCount.TotalDifferenceQuantity =
            totalDifference;

        stockCount.TotalDifferenceValue =
            totalDifferenceValue;

        stockCount.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var stockCount =
            await _context.StockCounts
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsActive);

        if (stockCount == null)
            return false;

        if (stockCount.IsPosted)
            throw new InvalidOperationException(
                "Posted stock count cannot be deleted.");

        stockCount.IsActive = false;
        stockCount.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PostAsync(int id)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var stockCount =
                await _context.StockCounts
                    .Include(x => x.StockCountLines)
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        x.IsActive);

            if (stockCount == null)
                return false;

            if (stockCount.IsPosted)
                throw new InvalidOperationException(
                    "Stock count is already posted.");

            if (stockCount.StockCountLines.Count == 0)
                throw new InvalidOperationException(
                    "Stock count has no lines.");

            foreach (var line in stockCount.StockCountLines
                         .OrderBy(x => x.LineNumber))
            {
                var product =
                    await _context.Products
                        .FirstOrDefaultAsync(x =>
                            x.Id == line.ProductId &&
                            x.IsActive);

                if (product == null)
                    throw new InvalidOperationException(
                        $"Product {line.ProductId} not found or inactive.");

                var balance =
                    await _context.StockBalances
                        .FirstOrDefaultAsync(x =>
                            x.ProductId == line.ProductId &&
                            x.WarehouseId == stockCount.WarehouseId &&
                            x.BranchId == stockCount.BranchId &&
                            x.IsActive);

                // No existing stock balance
                if (balance == null)
                {
                    if (line.CountedQuantity <= 0)
                        continue;

                    balance = new StockBalance
                    {
                        ProductId =
                            line.ProductId,

                        WarehouseId =
                            stockCount.WarehouseId,

                        BranchId =
                            stockCount.BranchId,

                        Quantity = 0,

                        ReservedQuantity = 0,

                        AvailableQuantity = 0,

                        AverageCost =
                            line.UnitCost,

                        StockValue = 0,

                        IsActive = true,

                        CreatedAt =
                            DateTime.UtcNow
                    };

                    _context.StockBalances.Add(balance);

                    await _context.SaveChangesAsync();
                }

                // Re-read actual current quantity
                var currentQuantity =
                    balance.Quantity;

                var difference =
                    line.CountedQuantity -
                    currentQuantity;

                // Update line with actual posting difference
                line.SystemQuantity =
                    currentQuantity;

                line.DifferenceQuantity =
                    difference;

                line.DifferenceValue =
                    difference *
                    line.UnitCost;

                if (difference > 0)
                {
                    // STOCK IN
                    var oldQuantity =
                        balance.Quantity;

                    var oldAverage =
                        balance.AverageCost;

                    var newQuantity =
                        oldQuantity +
                        difference;

                    if (newQuantity > 0)
                    {
                        balance.AverageCost =
                            (
                                (oldQuantity *
                                 oldAverage)
                                +
                                (difference *
                                 line.UnitCost)
                            )
                            /
                            newQuantity;
                    }

                    balance.Quantity =
                        newQuantity;

                    balance.AvailableQuantity =
                        balance.Quantity -
                        balance.ReservedQuantity;

                    balance.StockValue =
                        balance.Quantity *
                        balance.AverageCost;

                    var transactionIn =
                        new StockTransaction
                        {
                            ProductId =
                                line.ProductId,

                            WarehouseId =
                                stockCount.WarehouseId,

                            BranchId =
                                stockCount.BranchId,

                            FiscalYearId =
                                stockCount.FiscalYearId,

                            FiscalYearPeriodId =
                                stockCount.FiscalYearPeriodId,

                            TransactionDate =
                                stockCount.CountDate,

                            TransactionType =
                                "AdjustmentIn",

                            ReferenceType =
                                "StockCount",

                            ReferenceId =
                                stockCount.Id,

                            ReferenceNumber =
                                stockCount.CountNumber,

                            QuantityIn =
                                difference,

                            QuantityOut = 0,

                            BalanceQuantity =
                                balance.Quantity,

                            UnitCost =
                                line.UnitCost,

                            TotalCost =
                                difference *
                                line.UnitCost,

                            Description =
                                line.Description ??
                                stockCount.Reason,

                            IsActive = true,

                            CreatedAt =
                                DateTime.UtcNow
                        };

                    _context.StockTransactions.Add(
                        transactionIn);
                }
                else if (difference < 0)
                {
                    // STOCK OUT
                    var quantityOut =
                        Math.Abs(difference);

                    if (quantityOut >
                        balance.AvailableQuantity)
                    {
                        throw new InvalidOperationException(
                            $"Insufficient available stock for Product {line.ProductId}.");
                    }

                    var transactionUnitCost =
                        balance.AverageCost;

                    balance.Quantity -=
                        quantityOut;

                    balance.AvailableQuantity =
                        balance.Quantity -
                        balance.ReservedQuantity;

                    balance.StockValue =
                        balance.Quantity *
                        balance.AverageCost;

                    var transactionOut =
                        new StockTransaction
                        {
                            ProductId =
                                line.ProductId,

                            WarehouseId =
                                stockCount.WarehouseId,

                            BranchId =
                                stockCount.BranchId,

                            FiscalYearId =
                                stockCount.FiscalYearId,

                            FiscalYearPeriodId =
                                stockCount.FiscalYearPeriodId,

                            TransactionDate =
                                stockCount.CountDate,

                            TransactionType =
                                "AdjustmentOut",

                            ReferenceType =
                                "StockCount",

                            ReferenceId =
                                stockCount.Id,

                            ReferenceNumber =
                                stockCount.CountNumber,

                            QuantityIn = 0,

                            QuantityOut =
                                quantityOut,

                            BalanceQuantity =
                                balance.Quantity,

                            UnitCost =
                                transactionUnitCost,

                            TotalCost =
                                quantityOut *
                                transactionUnitCost,

                            Description =
                                line.Description ??
                                stockCount.Reason,

                            IsActive = true,

                            CreatedAt =
                                DateTime.UtcNow
                        };

                    _context.StockTransactions.Add(
                        transactionOut);
                }
            }

            stockCount.TotalSystemQuantity =
                stockCount.StockCountLines
                    .Sum(x => x.SystemQuantity);

            stockCount.TotalCountedQuantity =
                stockCount.StockCountLines
                    .Sum(x => x.CountedQuantity);

            stockCount.TotalDifferenceQuantity =
                stockCount.StockCountLines
                    .Sum(x => x.DifferenceQuantity);

            stockCount.TotalDifferenceValue =
                stockCount.StockCountLines
                    .Sum(x => x.DifferenceValue);

            stockCount.Status =
                "Posted";

            stockCount.IsPosted =
                true;

            stockCount.PostedAt =
                DateTime.UtcNow;

            stockCount.UpdatedAt =
                DateTime.UtcNow;

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
        StockCountDto dto)
    {
        if (dto.FiscalYearId <= 0)
            throw new ArgumentException(
                "FiscalYearId must be greater than zero.");

        if (dto.FiscalYearPeriodId <= 0)
            throw new ArgumentException(
                "FiscalYearPeriodId must be greater than zero.");

        if (string.IsNullOrWhiteSpace(dto.CountNumber))
            throw new ArgumentException(
                "CountNumber is required.");

        if (dto.CountDate == default)
            throw new ArgumentException(
                "CountDate is required.");
    }

    private static void ValidateLine(
        StockCountLineDto dto)
    {
        if (dto.ProductId <= 0)
            throw new ArgumentException(
                "ProductId must be greater than zero.");

        if (dto.CountedQuantity < 0)
            throw new ArgumentException(
                "CountedQuantity cannot be negative.");

        if (dto.UnitCost < 0)
            throw new ArgumentException(
                "UnitCost cannot be negative.");

        if (dto.LineNumber <= 0)
            throw new ArgumentException(
                "LineNumber must be greater than zero.");
    }

    private static StockCountDto MapToDto(
        StockCount stockCount)
    {
        return new StockCountDto
        {
            Id =
                stockCount.Id,

            FiscalYearId =
                stockCount.FiscalYearId,

            FiscalYearPeriodId =
                stockCount.FiscalYearPeriodId,

            WarehouseId =
                stockCount.WarehouseId,

            BranchId =
                stockCount.BranchId,

            CountNumber =
                stockCount.CountNumber,

            CountDate =
                stockCount.CountDate,

            ReferenceNumber =
                stockCount.ReferenceNumber,

            Reason =
                stockCount.Reason,

            TotalSystemQuantity =
                stockCount.TotalSystemQuantity,

            TotalCountedQuantity =
                stockCount.TotalCountedQuantity,

            TotalDifferenceQuantity =
                stockCount.TotalDifferenceQuantity,

            TotalDifferenceValue =
                stockCount.TotalDifferenceValue,

            Status =
                stockCount.Status,

            IsPosted =
                stockCount.IsPosted,

            PostedAt =
                stockCount.PostedAt,

            IsActive =
                stockCount.IsActive,

            CreatedAt =
                stockCount.CreatedAt,

            UpdatedAt =
                stockCount.UpdatedAt,

            Lines =
                stockCount.StockCountLines
                    .OrderBy(x => x.LineNumber)
                    .Select(x =>
                        new StockCountLineDto
                        {
                            Id =
                                x.Id,

                            StockCountId =
                                x.StockCountId,

                            ProductId =
                                x.ProductId,

                            SystemQuantity =
                                x.SystemQuantity,

                            CountedQuantity =
                                x.CountedQuantity,

                            DifferenceQuantity =
                                x.DifferenceQuantity,

                            UnitCost =
                                x.UnitCost,

                            DifferenceValue =
                                x.DifferenceValue,

                            Description =
                                x.Description,

                            LineNumber =
                                x.LineNumber
                        })
                    .ToList()
        };
    }
}