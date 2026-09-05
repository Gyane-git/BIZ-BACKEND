using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class StockTransactionService
    : IStockTransactionService
{
    private readonly TenantDbContext _context;

    private static readonly string[] AllowedTransactionTypes =
    {
        "Purchase",
        "PurchaseReturn",
        "Sale",
        "SalesReturn",
        "AdjustmentIn",
        "AdjustmentOut",
        "TransferIn",
        "TransferOut",
        "Opening"
    };

    public StockTransactionService(
        TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StockTransactionDto>>
        GetAllAsync()
    {
        return await _context.StockTransactions
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Id)
            .Select(x => MapDto(x))
            .ToListAsync();
    }

    public async Task<StockTransactionDto?>
        GetByIdAsync(int id)
    {
        return await _context.StockTransactions
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.IsActive)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<StockTransactionDto>>
        GetByProductAsync(int productId)
    {
        return await _context.StockTransactions
            .AsNoTracking()
            .Where(x =>
                x.ProductId == productId &&
                x.IsActive)
            .OrderBy(x => x.TransactionDate)
            .ThenBy(x => x.Id)
            .Select(x => MapDto(x))
            .ToListAsync();
    }

    public async Task<StockTransactionDto>
        CreateAsync(StockTransactionDto dto)
    {
        if (dto.ProductId <= 0)
            throw new ArgumentException(
                "ProductId must be greater than zero.");

        if (string.IsNullOrWhiteSpace(dto.TransactionType))
            throw new ArgumentException(
                "TransactionType is required.");

        if (!AllowedTransactionTypes.Contains(
                dto.TransactionType,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Invalid TransactionType.");
        }

        if (string.IsNullOrWhiteSpace(dto.ReferenceType))
            throw new ArgumentException(
                "ReferenceType is required.");

        if (dto.QuantityIn < 0)
            throw new ArgumentException(
                "QuantityIn cannot be negative.");

        if (dto.QuantityOut < 0)
            throw new ArgumentException(
                "QuantityOut cannot be negative.");

        if (dto.QuantityIn > 0 &&
            dto.QuantityOut > 0)
        {
            throw new ArgumentException(
                "A stock transaction cannot have both QuantityIn and QuantityOut.");
        }

        if (dto.QuantityIn == 0 &&
            dto.QuantityOut == 0)
        {
            throw new ArgumentException(
                "QuantityIn or QuantityOut must be greater than zero.");
        }

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

        var balance = await _context.StockBalances
            .FirstOrDefaultAsync(x =>
                x.ProductId == dto.ProductId &&
                x.WarehouseId == dto.WarehouseId &&
                x.BranchId == dto.BranchId &&
                x.IsActive);

        if (balance == null)
        {
            balance = new StockBalance
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                BranchId = dto.BranchId,
                Quantity = 0,
                ReservedQuantity = 0,
                AvailableQuantity = 0,
                AverageCost = 0,
                StockValue = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.StockBalances.Add(balance);
        }

        var newQuantity =
            balance.Quantity +
            dto.QuantityIn -
            dto.QuantityOut;

        if (newQuantity < 0)
        {
            throw new InvalidOperationException(
                $"Insufficient stock. Current stock: {balance.Quantity}, requested out: {dto.QuantityOut}.");
        }

        if (dto.QuantityOut > 0 &&
            dto.QuantityOut >
            balance.AvailableQuantity)
        {
            throw new InvalidOperationException(
                $"Insufficient available stock. Available: {balance.AvailableQuantity}, requested: {dto.QuantityOut}.");
        }

        decimal newAverageCost;

        if (dto.QuantityIn > 0)
        {
            var oldValue =
                balance.Quantity *
                balance.AverageCost;

            var incomingValue =
                dto.QuantityIn *
                dto.UnitCost;

            newAverageCost =
                newQuantity > 0
                    ? (oldValue + incomingValue) /
                      newQuantity
                    : dto.UnitCost;
        }
        else
        {
            newAverageCost =
                balance.AverageCost;
        }

        balance.Quantity = newQuantity;

        balance.AverageCost = newAverageCost;

        balance.StockValue =
            newQuantity * newAverageCost;

        balance.AvailableQuantity =
            newQuantity -
            balance.ReservedQuantity;

        balance.UpdatedAt = DateTime.UtcNow;

        var transaction = new StockTransaction
        {
            ProductId = dto.ProductId,

            WarehouseId = dto.WarehouseId,

            BranchId = dto.BranchId,

            FiscalYearId = dto.FiscalYearId,

            FiscalYearPeriodId =
                dto.FiscalYearPeriodId,

            TransactionDate =
                dto.TransactionDate == default
                    ? DateTime.UtcNow
                    : dto.TransactionDate,

            TransactionType =
                dto.TransactionType.Trim(),

            ReferenceType =
                dto.ReferenceType.Trim(),

            ReferenceId =
                dto.ReferenceId,

            ReferenceNumber =
                dto.ReferenceNumber,

            QuantityIn =
                dto.QuantityIn,

            QuantityOut =
                dto.QuantityOut,

            BalanceQuantity =
                newQuantity,

            UnitCost =
                dto.UnitCost,

            TotalCost =
                dto.QuantityIn > 0
                    ? dto.QuantityIn * dto.UnitCost
                    : dto.QuantityOut *
                      balance.AverageCost,

            Description =
                dto.Description,

            IsActive = true,

            CreatedAt = DateTime.UtcNow
        };

        _context.StockTransactions.Add(transaction);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(transaction.Id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var transaction = await _context.StockTransactions
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (transaction == null)
            return false;

        throw new InvalidOperationException(
            "Stock transactions cannot be deleted because they are inventory audit records.");
    }

    private static StockTransactionDto MapDto(
        StockTransaction x)
    {
        return new StockTransactionDto
        {
            Id = x.Id,
            ProductId = x.ProductId,
            WarehouseId = x.WarehouseId,
            BranchId = x.BranchId,
            FiscalYearId = x.FiscalYearId,
            FiscalYearPeriodId =
                x.FiscalYearPeriodId,
            TransactionDate =
                x.TransactionDate,
            TransactionType =
                x.TransactionType,
            ReferenceType =
                x.ReferenceType,
            ReferenceId =
                x.ReferenceId,
            ReferenceNumber =
                x.ReferenceNumber,
            QuantityIn =
                x.QuantityIn,
            QuantityOut =
                x.QuantityOut,
            BalanceQuantity =
                x.BalanceQuantity,
            UnitCost =
                x.UnitCost,
            TotalCost =
                x.TotalCost,
            Description =
                x.Description,
            IsActive =
                x.IsActive,
            CreatedAt =
                x.CreatedAt
        };
    }
}