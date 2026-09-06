using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class StockTransferLineService
    : IStockTransferLineService
{
    private readonly TenantDbContext _context;

    public StockTransferLineService(
        TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StockTransferLineDto>>
        GetAllAsync()
    {
        return await _context.StockTransferLines
            .AsNoTracking()
            .Where(x => x.StockTransfer.IsActive)
            .OrderBy(x => x.StockTransferId)
            .ThenBy(x => x.LineNumber)
            .Select(x => new StockTransferLineDto
            {
                Id = x.Id,
                StockTransferId = x.StockTransferId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                LineTotal = x.LineTotal,
                Description = x.Description,
                LineNumber = x.LineNumber
            })
            .ToListAsync();
    }

    public async Task<StockTransferLineDto?>
        GetByIdAsync(int id)
    {
        return await _context.StockTransferLines
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.StockTransfer.IsActive)
            .Select(x => new StockTransferLineDto
            {
                Id = x.Id,
                StockTransferId = x.StockTransferId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                LineTotal = x.LineTotal,
                Description = x.Description,
                LineNumber = x.LineNumber
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<StockTransferLineDto>>
        GetByTransferAsync(int stockTransferId)
    {
        return await _context.StockTransferLines
            .AsNoTracking()
            .Where(x =>
                x.StockTransferId == stockTransferId &&
                x.StockTransfer.IsActive)
            .OrderBy(x => x.LineNumber)
            .Select(x => new StockTransferLineDto
            {
                Id = x.Id,
                StockTransferId = x.StockTransferId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                LineTotal = x.LineTotal,
                Description = x.Description,
                LineNumber = x.LineNumber
            })
            .ToListAsync();
    }

    public async Task<StockTransferLineDto>
        CreateAsync(StockTransferLineDto dto)
    {
        ValidateLine(dto);

        var transfer =
            await _context.StockTransfers
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.StockTransferId &&
                    x.IsActive);

        if (transfer == null)
            throw new ArgumentException(
                "Stock transfer not found or inactive.");

        if (transfer.IsPosted)
            throw new InvalidOperationException(
                "Cannot add line to a posted stock transfer.");

        var product =
            await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.ProductId &&
                    x.IsActive);

        if (product == null)
            throw new ArgumentException(
                "Product not found or inactive.");

        var duplicate =
            await _context.StockTransferLines
                .AnyAsync(x =>
                    x.StockTransferId ==
                        dto.StockTransferId &&
                    x.LineNumber ==
                        dto.LineNumber);

        if (duplicate)
            throw new ArgumentException(
                "LineNumber already exists.");

        var line = new StockTransferLine
        {
            StockTransferId =
                dto.StockTransferId,

            ProductId =
                dto.ProductId,

            Quantity =
                dto.Quantity,

            UnitCost =
                dto.UnitCost,

            LineTotal =
                dto.Quantity * dto.UnitCost,

            Description =
                dto.Description?.Trim(),

            LineNumber =
                dto.LineNumber
        };

        _context.StockTransferLines.Add(line);

        await _context.SaveChangesAsync();

        await RecalculateHeaderAsync(
            dto.StockTransferId);

        return (await GetByIdAsync(line.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        StockTransferLineDto dto)
    {
        var line =
            await _context.StockTransferLines
                .Include(x => x.StockTransfer)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.StockTransfer.IsActive);

        if (line == null)
            return false;

        if (line.StockTransfer.IsPosted)
            throw new InvalidOperationException(
                "Cannot update line of a posted stock transfer.");

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
            await _context.StockTransferLines
                .AnyAsync(x =>
                    x.StockTransferId ==
                        line.StockTransferId &&
                    x.LineNumber ==
                        dto.LineNumber &&
                    x.Id != id);

        if (duplicate)
            throw new ArgumentException(
                "LineNumber already exists.");

        line.ProductId =
            dto.ProductId;

        line.Quantity =
            dto.Quantity;

        line.UnitCost =
            dto.UnitCost;

        line.LineTotal =
            dto.Quantity * dto.UnitCost;

        line.Description =
            dto.Description?.Trim();

        line.LineNumber =
            dto.LineNumber;

        await _context.SaveChangesAsync();

        await RecalculateHeaderAsync(
            line.StockTransferId);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var line =
            await _context.StockTransferLines
                .Include(x => x.StockTransfer)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.StockTransfer.IsActive);

        if (line == null)
            return false;

        if (line.StockTransfer.IsPosted)
            throw new InvalidOperationException(
                "Cannot delete line of a posted stock transfer.");

        var transferId =
            line.StockTransferId;

        _context.StockTransferLines.Remove(line);

        await _context.SaveChangesAsync();

        await RecalculateHeaderAsync(
            transferId);

        return true;
    }

    private async Task RecalculateHeaderAsync(
        int transferId)
    {
        var transfer =
            await _context.StockTransfers
                .FirstOrDefaultAsync(x =>
                    x.Id == transferId &&
                    x.IsActive);

        if (transfer == null)
            return;

        var lines =
            await _context.StockTransferLines
                .Where(x =>
                    x.StockTransferId ==
                    transferId)
                .ToListAsync();

        transfer.TotalQuantity =
            lines.Sum(x => x.Quantity);

        transfer.TotalValue =
            lines.Sum(x => x.LineTotal);

        transfer.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private static void ValidateLine(
        StockTransferLineDto dto)
    {
        if (dto.StockTransferId <= 0)
            throw new ArgumentException(
                "StockTransferId must be greater than zero.");

        if (dto.ProductId <= 0)
            throw new ArgumentException(
                "ProductId must be greater than zero.");

        if (dto.Quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        if (dto.UnitCost < 0)
            throw new ArgumentException(
                "UnitCost cannot be negative.");

        if (dto.LineNumber <= 0)
            throw new ArgumentException(
                "LineNumber must be greater than zero.");
    }
}