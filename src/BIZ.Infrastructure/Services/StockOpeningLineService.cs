using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class StockOpeningLineService : IStockOpeningLineService
{
    private readonly TenantDbContext _context;

    public StockOpeningLineService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StockOpeningLineDto>> GetAllAsync()
    {
        return await _context.StockOpeningLines
            .AsNoTracking()
            .Where(x => x.StockOpening.IsActive)
            .OrderBy(x => x.StockOpeningId)
            .ThenBy(x => x.LineNumber)
            .Select(x => new StockOpeningLineDto
            {
                Id = x.Id,
                StockOpeningId = x.StockOpeningId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                TotalCost = x.TotalCost,
                Description = x.Description,
                LineNumber = x.LineNumber
            })
            .ToListAsync();
    }

    public async Task<StockOpeningLineDto?> GetByIdAsync(int id)
    {
        return await _context.StockOpeningLines
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.StockOpening.IsActive)
            .Select(x => new StockOpeningLineDto
            {
                Id = x.Id,
                StockOpeningId = x.StockOpeningId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                TotalCost = x.TotalCost,
                Description = x.Description,
                LineNumber = x.LineNumber
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<StockOpeningLineDto>> GetByOpeningAsync(
        int stockOpeningId)
    {
        return await _context.StockOpeningLines
            .AsNoTracking()
            .Where(x =>
                x.StockOpeningId == stockOpeningId &&
                x.StockOpening.IsActive)
            .OrderBy(x => x.LineNumber)
            .Select(x => new StockOpeningLineDto
            {
                Id = x.Id,
                StockOpeningId = x.StockOpeningId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                TotalCost = x.TotalCost,
                Description = x.Description,
                LineNumber = x.LineNumber
            })
            .ToListAsync();
    }

    public async Task<StockOpeningLineDto> CreateAsync(
        StockOpeningLineDto dto)
    {
        var opening = await _context.StockOpenings
            .FirstOrDefaultAsync(x =>
                x.Id == dto.StockOpeningId &&
                x.IsActive);

        if (opening == null)
            throw new InvalidOperationException(
                "Stock opening not found.");

        if (opening.IsPosted)
            throw new InvalidOperationException(
                "Cannot add line to a posted stock opening.");

        if (dto.ProductId <= 0)
            throw new ArgumentException(
                "Valid ProductId is required.");

        if (dto.Quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        if (dto.UnitCost < 0)
            throw new ArgumentException(
                "UnitCost cannot be negative.");

        var productExists = await _context.Products
            .AnyAsync(x =>
                x.Id == dto.ProductId &&
                x.IsActive);

        if (!productExists)
            throw new InvalidOperationException(
                "Product not found or inactive.");

        var duplicateLine = await _context.StockOpeningLines
            .AnyAsync(x =>
                x.StockOpeningId == dto.StockOpeningId &&
                x.LineNumber == dto.LineNumber);

        if (duplicateLine)
            throw new InvalidOperationException(
                $"Line number {dto.LineNumber} already exists.");

        var totalCost =
            dto.Quantity * dto.UnitCost;

        var line = new StockOpeningLine
        {
            StockOpeningId = dto.StockOpeningId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            UnitCost = dto.UnitCost,
            TotalCost = totalCost,
            Description = dto.Description?.Trim(),
            LineNumber = dto.LineNumber
        };

        _context.StockOpeningLines.Add(line);

        await RecalculateHeaderAsync(opening);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(line.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        StockOpeningLineDto dto)
    {
        var line = await _context.StockOpeningLines
            .Include(x => x.StockOpening)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.StockOpening.IsActive);

        if (line == null)
            return false;

        if (line.StockOpening.IsPosted)
            throw new InvalidOperationException(
                "Cannot update line of a posted stock opening.");

        if (dto.ProductId <= 0)
            throw new ArgumentException(
                "Valid ProductId is required.");

        if (dto.Quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        if (dto.UnitCost < 0)
            throw new ArgumentException(
                "UnitCost cannot be negative.");

        var productExists = await _context.Products
            .AnyAsync(x =>
                x.Id == dto.ProductId &&
                x.IsActive);

        if (!productExists)
            throw new InvalidOperationException(
                "Product not found or inactive.");

        var duplicateLine = await _context.StockOpeningLines
            .AnyAsync(x =>
                x.Id != id &&
                x.StockOpeningId == line.StockOpeningId &&
                x.LineNumber == dto.LineNumber);

        if (duplicateLine)
            throw new InvalidOperationException(
                $"Line number {dto.LineNumber} already exists.");

        line.ProductId = dto.ProductId;
        line.Quantity = dto.Quantity;
        line.UnitCost = dto.UnitCost;
        line.TotalCost = dto.Quantity * dto.UnitCost;
        line.Description = dto.Description?.Trim();
        line.LineNumber = dto.LineNumber;

        await RecalculateHeaderAsync(line.StockOpening);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var line = await _context.StockOpeningLines
            .Include(x => x.StockOpening)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.StockOpening.IsActive);

        if (line == null)
            return false;

        if (line.StockOpening.IsPosted)
            throw new InvalidOperationException(
                "Cannot delete line of a posted stock opening.");

        var opening = line.StockOpening;

        _context.StockOpeningLines.Remove(line);

        await RecalculateHeaderAsync(
            opening,
            excludeLineId: id);

        await _context.SaveChangesAsync();

        return true;
    }

    private async Task RecalculateHeaderAsync(
        StockOpening opening,
        int? excludeLineId = null)
    {
        var query = _context.StockOpeningLines
            .Where(x => x.StockOpeningId == opening.Id);

        if (excludeLineId.HasValue)
        {
            query = query.Where(x =>
                x.Id != excludeLineId.Value);
        }

        var totals = await query
            .GroupBy(x => x.StockOpeningId)
            .Select(g => new
            {
                TotalQuantity = g.Sum(x => x.Quantity),
                TotalValue = g.Sum(x => x.TotalCost)
            })
            .FirstOrDefaultAsync();

        opening.TotalQuantity =
            totals?.TotalQuantity ?? 0;

        opening.TotalValue =
            totals?.TotalValue ?? 0;

        opening.UpdatedAt = DateTime.UtcNow;
    }
}