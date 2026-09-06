using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class StockCountLineService
    : IStockCountLineService
{
    private readonly TenantDbContext _context;

    public StockCountLineService(
        TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StockCountLineDto>>
        GetAllAsync()
    {
        return await _context.StockCountLines
            .AsNoTracking()
            .Where(x => x.StockCount.IsActive)
            .OrderBy(x => x.StockCountId)
            .ThenBy(x => x.LineNumber)
            .Select(x => new StockCountLineDto
            {
                Id = x.Id,
                StockCountId = x.StockCountId,
                ProductId = x.ProductId,
                SystemQuantity = x.SystemQuantity,
                CountedQuantity = x.CountedQuantity,
                DifferenceQuantity = x.DifferenceQuantity,
                UnitCost = x.UnitCost,
                DifferenceValue = x.DifferenceValue,
                Description = x.Description,
                LineNumber = x.LineNumber
            })
            .ToListAsync();
    }

    public async Task<StockCountLineDto?>
        GetByIdAsync(int id)
    {
        return await _context.StockCountLines
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.StockCount.IsActive)
            .Select(x => new StockCountLineDto
            {
                Id = x.Id,
                StockCountId = x.StockCountId,
                ProductId = x.ProductId,
                SystemQuantity = x.SystemQuantity,
                CountedQuantity = x.CountedQuantity,
                DifferenceQuantity = x.DifferenceQuantity,
                UnitCost = x.UnitCost,
                DifferenceValue = x.DifferenceValue,
                Description = x.Description,
                LineNumber = x.LineNumber
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<StockCountLineDto>>
        GetByCountAsync(int stockCountId)
    {
        return await _context.StockCountLines
            .AsNoTracking()
            .Where(x =>
                x.StockCountId == stockCountId &&
                x.StockCount.IsActive)
            .OrderBy(x => x.LineNumber)
            .Select(x => new StockCountLineDto
            {
                Id = x.Id,
                StockCountId = x.StockCountId,
                ProductId = x.ProductId,
                SystemQuantity = x.SystemQuantity,
                CountedQuantity = x.CountedQuantity,
                DifferenceQuantity = x.DifferenceQuantity,
                UnitCost = x.UnitCost,
                DifferenceValue = x.DifferenceValue,
                Description = x.Description,
                LineNumber = x.LineNumber
            })
            .ToListAsync();
    }

    public async Task<StockCountLineDto>
        CreateAsync(StockCountLineDto dto)
    {
        ValidateLine(dto);

        var stockCount =
            await _context.StockCounts
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.StockCountId &&
                    x.IsActive);

        if (stockCount == null)
            throw new ArgumentException(
                "Stock count not found or inactive.");

        if (stockCount.IsPosted)
            throw new InvalidOperationException(
                "Cannot add line to posted stock count.");

        var product =
            await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.ProductId &&
                    x.IsActive);

        if (product == null)
            throw new ArgumentException(
                "Product not found or inactive.");

        var duplicate =
            await _context.StockCountLines
                .AnyAsync(x =>
                    x.StockCountId ==
                        dto.StockCountId &&
                    x.LineNumber ==
                        dto.LineNumber);

        if (duplicate)
            throw new ArgumentException(
                "LineNumber already exists.");

        var balance =
            await _context.StockBalances
                .FirstOrDefaultAsync(x =>
                    x.ProductId == dto.ProductId &&
                    x.WarehouseId == stockCount.WarehouseId &&
                    x.BranchId == stockCount.BranchId &&
                    x.IsActive);

        var systemQuantity =
            balance?.Quantity ?? 0;

        var difference =
            dto.CountedQuantity -
            systemQuantity;

        var differenceValue =
            difference *
            dto.UnitCost;

        var line = new StockCountLine
        {
            StockCountId =
                dto.StockCountId,

            ProductId =
                dto.ProductId,

            SystemQuantity =
                systemQuantity,

            CountedQuantity =
                dto.CountedQuantity,

            DifferenceQuantity =
                difference,

            UnitCost =
                dto.UnitCost,

            DifferenceValue =
                differenceValue,

            Description =
                dto.Description?.Trim(),

            LineNumber =
                dto.LineNumber
        };

        _context.StockCountLines.Add(line);

        await _context.SaveChangesAsync();

        await RecalculateHeaderAsync(
            dto.StockCountId);

        return (await GetByIdAsync(line.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        StockCountLineDto dto)
    {
        var line =
            await _context.StockCountLines
                .Include(x => x.StockCount)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.StockCount.IsActive);

        if (line == null)
            return false;

        if (line.StockCount.IsPosted)
            throw new InvalidOperationException(
                "Cannot update line of posted stock count.");

        ValidateLine(dto);

        var product =
            await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.ProductId &&
                    x.IsActive);

        if (product == null)
            throw new ArgumentException(
                "Product not found or inactive.");

        var duplicate =
            await _context.StockCountLines
                .AnyAsync(x =>
                    x.StockCountId ==
                        line.StockCountId &&
                    x.LineNumber ==
                        dto.LineNumber &&
                    x.Id != id);

        if (duplicate)
            throw new ArgumentException(
                "LineNumber already exists.");

        var balance =
            await _context.StockBalances
                .FirstOrDefaultAsync(x =>
                    x.ProductId == dto.ProductId &&
                    x.WarehouseId ==
                        line.StockCount.WarehouseId &&
                    x.BranchId ==
                        line.StockCount.BranchId &&
                    x.IsActive);

        var systemQuantity =
            balance?.Quantity ?? 0;

        var difference =
            dto.CountedQuantity -
            systemQuantity;

        var differenceValue =
            difference *
            dto.UnitCost;

        line.ProductId =
            dto.ProductId;

        line.SystemQuantity =
            systemQuantity;

        line.CountedQuantity =
            dto.CountedQuantity;

        line.DifferenceQuantity =
            difference;

        line.UnitCost =
            dto.UnitCost;

        line.DifferenceValue =
            differenceValue;

        line.Description =
            dto.Description?.Trim();

        line.LineNumber =
            dto.LineNumber;

        await _context.SaveChangesAsync();

        await RecalculateHeaderAsync(
            line.StockCountId);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var line =
            await _context.StockCountLines
                .Include(x => x.StockCount)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.StockCount.IsActive);

        if (line == null)
            return false;

        if (line.StockCount.IsPosted)
            throw new InvalidOperationException(
                "Cannot delete line of posted stock count.");

        var countId =
            line.StockCountId;

        _context.StockCountLines.Remove(line);

        await _context.SaveChangesAsync();

        await RecalculateHeaderAsync(countId);

        return true;
    }

    private async Task RecalculateHeaderAsync(
        int stockCountId)
    {
        var stockCount =
            await _context.StockCounts
                .FirstOrDefaultAsync(x =>
                    x.Id == stockCountId &&
                    x.IsActive);

        if (stockCount == null)
            return;

        var lines =
            await _context.StockCountLines
                .Where(x =>
                    x.StockCountId ==
                    stockCountId)
                .ToListAsync();

        stockCount.TotalSystemQuantity =
            lines.Sum(x => x.SystemQuantity);

        stockCount.TotalCountedQuantity =
            lines.Sum(x => x.CountedQuantity);

        stockCount.TotalDifferenceQuantity =
            lines.Sum(x => x.DifferenceQuantity);

        stockCount.TotalDifferenceValue =
            lines.Sum(x => x.DifferenceValue);

        stockCount.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private static void ValidateLine(
        StockCountLineDto dto)
    {
        if (dto.StockCountId <= 0)
            throw new ArgumentException(
                "StockCountId must be greater than zero.");

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
}