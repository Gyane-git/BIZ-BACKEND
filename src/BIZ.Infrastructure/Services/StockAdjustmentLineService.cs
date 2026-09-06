using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class StockAdjustmentLineService : IStockAdjustmentLineService
{
    private readonly TenantDbContext _context;

    public StockAdjustmentLineService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StockAdjustmentLineDto>> GetAllAsync()
    {
        return await _context.StockAdjustmentLines
            .AsNoTracking()
            .Where(x => x.StockAdjustment.IsActive)
            .OrderBy(x => x.StockAdjustmentId)
            .ThenBy(x => x.LineNumber)
            .Select(x => MapExpression(x))
            .ToListAsync();
    }

    public async Task<StockAdjustmentLineDto?> GetByIdAsync(int id)
    {
        return await _context.StockAdjustmentLines
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.StockAdjustment.IsActive)
            .Select(x => MapExpression(x))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<StockAdjustmentLineDto>>
        GetByAdjustmentAsync(int stockAdjustmentId)
    {
        return await _context.StockAdjustmentLines
            .AsNoTracking()
            .Where(x =>
                x.StockAdjustmentId == stockAdjustmentId &&
                x.StockAdjustment.IsActive)
            .OrderBy(x => x.LineNumber)
            .Select(x => MapExpression(x))
            .ToListAsync();
    }

    public async Task<StockAdjustmentLineDto> CreateAsync(
        StockAdjustmentLineDto dto)
    {
        if (dto.StockAdjustmentId <= 0)
            throw new ArgumentException(
                "StockAdjustmentId must be greater than zero.");

        if (dto.ProductId <= 0)
            throw new ArgumentException(
                "ProductId must be greater than zero.");

        if (dto.Quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        if (dto.UnitCost < 0)
            throw new ArgumentException(
                "UnitCost cannot be negative.");

        var adjustment = await _context.StockAdjustments
            .FirstOrDefaultAsync(x =>
                x.Id == dto.StockAdjustmentId &&
                x.IsActive);

        if (adjustment == null)
            throw new ArgumentException(
                "Stock adjustment not found or inactive.");

        if (adjustment.IsPosted)
            throw new InvalidOperationException(
                "Cannot add line to a posted stock adjustment.");

        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.Id == dto.ProductId &&
                x.IsActive);

        if (product == null)
            throw new ArgumentException(
                "Product not found or inactive.");

        var adjustmentType =
            dto.AdjustmentType.Trim();

        if (adjustmentType != "Increase" &&
            adjustmentType != "Decrease")
        {
            throw new ArgumentException(
                "AdjustmentType must be Increase or Decrease.");
        }

        var duplicateLine =
            await _context.StockAdjustmentLines.AnyAsync(x =>
                x.StockAdjustmentId == dto.StockAdjustmentId &&
                x.LineNumber == dto.LineNumber);

        if (duplicateLine)
            throw new ArgumentException(
                "LineNumber already exists for this stock adjustment.");

        var line = new StockAdjustmentLine
        {
            StockAdjustmentId = dto.StockAdjustmentId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            UnitCost = dto.UnitCost,
            LineTotal = dto.Quantity * dto.UnitCost,
            AdjustmentType = adjustmentType,
            Description = dto.Description?.Trim(),
            LineNumber = dto.LineNumber
        };

        _context.StockAdjustmentLines.Add(line);

        await _context.SaveChangesAsync();

        await RecalculateHeaderAsync(
            dto.StockAdjustmentId);

        return (await GetByIdAsync(line.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        StockAdjustmentLineDto dto)
    {
        var line = await _context.StockAdjustmentLines
            .Include(x => x.StockAdjustment)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.StockAdjustment.IsActive);

        if (line == null)
            return false;

        if (line.StockAdjustment.IsPosted)
            throw new InvalidOperationException(
                "Cannot update line of a posted stock adjustment.");

        if (dto.ProductId <= 0)
            throw new ArgumentException(
                "ProductId must be greater than zero.");

        if (dto.Quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        if (dto.UnitCost < 0)
            throw new ArgumentException(
                "UnitCost cannot be negative.");

        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.Id == dto.ProductId &&
                x.IsActive);

        if (product == null)
            throw new ArgumentException(
                "Product not found or inactive.");

        var adjustmentType =
            dto.AdjustmentType.Trim();

        if (adjustmentType != "Increase" &&
            adjustmentType != "Decrease")
        {
            throw new ArgumentException(
                "AdjustmentType must be Increase or Decrease.");
        }

        var duplicateLine =
            await _context.StockAdjustmentLines.AnyAsync(x =>
                x.StockAdjustmentId == line.StockAdjustmentId &&
                x.LineNumber == dto.LineNumber &&
                x.Id != id);

        if (duplicateLine)
            throw new ArgumentException(
                "LineNumber already exists for this stock adjustment.");

        line.ProductId = dto.ProductId;
        line.Quantity = dto.Quantity;
        line.UnitCost = dto.UnitCost;
        line.LineTotal = dto.Quantity * dto.UnitCost;
        line.AdjustmentType = adjustmentType;
        line.Description = dto.Description?.Trim();
        line.LineNumber = dto.LineNumber;

        await _context.SaveChangesAsync();

        await RecalculateHeaderAsync(
            line.StockAdjustmentId);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var line = await _context.StockAdjustmentLines
            .Include(x => x.StockAdjustment)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.StockAdjustment.IsActive);

        if (line == null)
            return false;

        if (line.StockAdjustment.IsPosted)
            throw new InvalidOperationException(
                "Cannot delete line of a posted stock adjustment.");

        var adjustmentId = line.StockAdjustmentId;

        _context.StockAdjustmentLines.Remove(line);

        await _context.SaveChangesAsync();

        await RecalculateHeaderAsync(adjustmentId);

        return true;
    }

    private async Task RecalculateHeaderAsync(
        int adjustmentId)
    {
        var adjustment = await _context.StockAdjustments
            .FirstOrDefaultAsync(x =>
                x.Id == adjustmentId &&
                x.IsActive);

        if (adjustment == null)
            return;

        var lines = await _context.StockAdjustmentLines
            .Where(x => x.StockAdjustmentId == adjustmentId)
            .ToListAsync();

        adjustment.TotalIncreaseQuantity =
            lines
                .Where(x => x.AdjustmentType == "Increase")
                .Sum(x => x.Quantity);

        adjustment.TotalDecreaseQuantity =
            lines
                .Where(x => x.AdjustmentType == "Decrease")
                .Sum(x => x.Quantity);

        adjustment.TotalValue =
            lines.Sum(x => x.LineTotal);

        adjustment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private static StockAdjustmentLineDto MapExpression(
        StockAdjustmentLine x)
    {
        return new StockAdjustmentLineDto
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
        };
    }
}