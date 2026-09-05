using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class StockBalanceService : IStockBalanceService
{
    private readonly TenantDbContext _context;

    public StockBalanceService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StockBalanceDto>> GetAllAsync()
    {
        return await _context.StockBalances
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.ProductId)
            .Select(x => new StockBalanceDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                WarehouseId = x.WarehouseId,
                BranchId = x.BranchId,
                Quantity = x.Quantity,
                ReservedQuantity = x.ReservedQuantity,
                AvailableQuantity = x.AvailableQuantity,
                AverageCost = x.AverageCost,
                StockValue = x.StockValue,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<StockBalanceDto?> GetByIdAsync(int id)
    {
        return await _context.StockBalances
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.IsActive)
            .Select(x => new StockBalanceDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                WarehouseId = x.WarehouseId,
                BranchId = x.BranchId,
                Quantity = x.Quantity,
                ReservedQuantity = x.ReservedQuantity,
                AvailableQuantity = x.AvailableQuantity,
                AverageCost = x.AverageCost,
                StockValue = x.StockValue,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<StockBalanceDto>>
        GetByProductAsync(int productId)
    {
        return await _context.StockBalances
            .AsNoTracking()
            .Where(x =>
                x.ProductId == productId &&
                x.IsActive)
            .Select(x => new StockBalanceDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                WarehouseId = x.WarehouseId,
                BranchId = x.BranchId,
                Quantity = x.Quantity,
                ReservedQuantity = x.ReservedQuantity,
                AvailableQuantity = x.AvailableQuantity,
                AverageCost = x.AverageCost,
                StockValue = x.StockValue,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<StockBalanceDto>>
        GetByWarehouseAsync(int warehouseId)
    {
        return await _context.StockBalances
            .AsNoTracking()
            .Where(x =>
                x.WarehouseId == warehouseId &&
                x.IsActive)
            .Select(x => new StockBalanceDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                WarehouseId = x.WarehouseId,
                BranchId = x.BranchId,
                Quantity = x.Quantity,
                ReservedQuantity = x.ReservedQuantity,
                AvailableQuantity = x.AvailableQuantity,
                AverageCost = x.AverageCost,
                StockValue = x.StockValue,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<StockBalanceDto> CreateAsync(
        StockBalanceDto dto)
    {
        if (dto.ProductId <= 0)
            throw new ArgumentException(
                "ProductId must be greater than zero.");

        if (dto.Quantity < 0)
            throw new ArgumentException(
                "Quantity cannot be negative.");

        if (dto.ReservedQuantity < 0)
            throw new ArgumentException(
                "ReservedQuantity cannot be negative.");

        if (dto.ReservedQuantity > dto.Quantity)
            throw new ArgumentException(
                "ReservedQuantity cannot exceed Quantity.");

        if (dto.AverageCost < 0)
            throw new ArgumentException(
                "AverageCost cannot be negative.");

        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.Id == dto.ProductId &&
                x.IsActive);

        if (product == null)
            throw new ArgumentException(
                "Product not found or inactive.");

        var duplicate = await _context.StockBalances
            .AnyAsync(x =>
                x.ProductId == dto.ProductId &&
                x.WarehouseId == dto.WarehouseId &&
                x.BranchId == dto.BranchId &&
                x.IsActive);

        if (duplicate)
            throw new ArgumentException(
                "Stock balance already exists for this Product/Warehouse/Branch.");

        var balance = new StockBalance
        {
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            BranchId = dto.BranchId,
            Quantity = dto.Quantity,
            ReservedQuantity = dto.ReservedQuantity,
            AvailableQuantity =
                dto.Quantity - dto.ReservedQuantity,
            AverageCost = dto.AverageCost,
            StockValue =
                dto.Quantity * dto.AverageCost,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.StockBalances.Add(balance);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(balance.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        StockBalanceDto dto)
    {
        var balance = await _context.StockBalances
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (balance == null)
            return false;

        if (dto.Quantity < 0)
            throw new ArgumentException(
                "Quantity cannot be negative.");

        if (dto.ReservedQuantity < 0)
            throw new ArgumentException(
                "ReservedQuantity cannot be negative.");

        if (dto.ReservedQuantity > dto.Quantity)
            throw new ArgumentException(
                "ReservedQuantity cannot exceed Quantity.");

        if (dto.AverageCost < 0)
            throw new ArgumentException(
                "AverageCost cannot be negative.");

        balance.Quantity = dto.Quantity;

        balance.ReservedQuantity =
            dto.ReservedQuantity;

        balance.AvailableQuantity =
            dto.Quantity - dto.ReservedQuantity;

        balance.AverageCost =
            dto.AverageCost;

        balance.StockValue =
            dto.Quantity * dto.AverageCost;

        balance.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var balance = await _context.StockBalances
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (balance == null)
            return false;

        balance.IsActive = false;
        balance.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}