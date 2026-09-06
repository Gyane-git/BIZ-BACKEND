using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class StockTransferService : IStockTransferService
{
    private readonly TenantDbContext _context;

    public StockTransferService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StockTransferDto>> GetAllAsync()
    {
        return await _context.StockTransfers
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.TransferDate)
            .ThenByDescending(x => x.Id)
            .Select(x => new StockTransferDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                FromWarehouseId = x.FromWarehouseId,
                ToWarehouseId = x.ToWarehouseId,
                FromBranchId = x.FromBranchId,
                ToBranchId = x.ToBranchId,
                TransferNumber = x.TransferNumber,
                TransferDate = x.TransferDate,
                ReferenceNumber = x.ReferenceNumber,
                Reason = x.Reason,
                TotalQuantity = x.TotalQuantity,
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

    public async Task<StockTransferDto?> GetByIdAsync(int id)
    {
        var transfer = await _context.StockTransfers
            .AsNoTracking()
            .Include(x => x.StockTransferLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (transfer == null)
            return null;

        return MapToDto(transfer);
    }

    public async Task<StockTransferDto> CreateAsync(
        StockTransferDto dto)
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

        if (dto.TransferDate.Date < period.StartDate.Date ||
            dto.TransferDate.Date > period.EndDate.Date)
        {
            throw new ArgumentException(
                "Transfer date must be within fiscal year period.");
        }

        var duplicate = await _context.StockTransfers
            .AnyAsync(x =>
                x.TransferNumber == dto.TransferNumber.Trim().ToUpperInvariant() &&
                x.IsActive);

        if (duplicate)
            throw new ArgumentException(
                "Transfer number already exists.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException(
                "At least one transfer line is required.");

        var transfer = new StockTransfer
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,

            FromWarehouseId = dto.FromWarehouseId,
            ToWarehouseId = dto.ToWarehouseId,

            FromBranchId = dto.FromBranchId,
            ToBranchId = dto.ToBranchId,

            TransferNumber =
                dto.TransferNumber.Trim().ToUpperInvariant(),

            TransferDate = dto.TransferDate,

            ReferenceNumber =
                dto.ReferenceNumber?.Trim(),

            Reason =
                dto.Reason?.Trim(),

            Status = "Draft",

            IsPosted = false,

            IsActive = true,

            CreatedAt = DateTime.UtcNow
        };

        decimal totalQuantity = 0;
        decimal totalValue = 0;

        foreach (var lineDto in dto.Lines
                     .OrderBy(x => x.LineNumber))
        {
            if (lineDto.ProductId <= 0)
                throw new ArgumentException(
                    "ProductId must be greater than zero.");

            if (lineDto.Quantity <= 0)
                throw new ArgumentException(
                    "Quantity must be greater than zero.");

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

            var lineTotal =
                lineDto.Quantity * lineDto.UnitCost;

            transfer.StockTransferLines.Add(
                new StockTransferLine
                {
                    ProductId = lineDto.ProductId,
                    Quantity = lineDto.Quantity,
                    UnitCost = lineDto.UnitCost,
                    LineTotal = lineTotal,
                    Description =
                        lineDto.Description?.Trim(),
                    LineNumber = lineDto.LineNumber
                });

            totalQuantity += lineDto.Quantity;
            totalValue += lineTotal;
        }

        transfer.TotalQuantity = totalQuantity;
        transfer.TotalValue = totalValue;

        _context.StockTransfers.Add(transfer);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(transfer.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        StockTransferDto dto)
    {
        var transfer = await _context.StockTransfers
            .Include(x => x.StockTransferLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (transfer == null)
            return false;

        if (transfer.IsPosted)
            throw new InvalidOperationException(
                "Posted stock transfer cannot be updated.");

        ValidateHeader(dto);

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException(
                "At least one transfer line is required.");

        transfer.FiscalYearId = dto.FiscalYearId;
        transfer.FiscalYearPeriodId = dto.FiscalYearPeriodId;

        transfer.FromWarehouseId = dto.FromWarehouseId;
        transfer.ToWarehouseId = dto.ToWarehouseId;

        transfer.FromBranchId = dto.FromBranchId;
        transfer.ToBranchId = dto.ToBranchId;

        transfer.TransferNumber =
            dto.TransferNumber.Trim().ToUpperInvariant();

        transfer.TransferDate = dto.TransferDate;

        transfer.ReferenceNumber =
            dto.ReferenceNumber?.Trim();

        transfer.Reason =
            dto.Reason?.Trim();

        _context.StockTransferLines.RemoveRange(
            transfer.StockTransferLines);

        decimal totalQuantity = 0;
        decimal totalValue = 0;

        foreach (var lineDto in dto.Lines
                     .OrderBy(x => x.LineNumber))
        {
            if (lineDto.ProductId <= 0)
                throw new ArgumentException(
                    "ProductId must be greater than zero.");

            if (lineDto.Quantity <= 0)
                throw new ArgumentException(
                    "Quantity must be greater than zero.");

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

            var lineTotal =
                lineDto.Quantity * lineDto.UnitCost;

            transfer.StockTransferLines.Add(
                new StockTransferLine
                {
                    ProductId = lineDto.ProductId,
                    Quantity = lineDto.Quantity,
                    UnitCost = lineDto.UnitCost,
                    LineTotal = lineTotal,
                    Description =
                        lineDto.Description?.Trim(),
                    LineNumber = lineDto.LineNumber
                });

            totalQuantity += lineDto.Quantity;
            totalValue += lineTotal;
        }

        transfer.TotalQuantity = totalQuantity;
        transfer.TotalValue = totalValue;
        transfer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var transfer = await _context.StockTransfers
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (transfer == null)
            return false;

        if (transfer.IsPosted)
            throw new InvalidOperationException(
                "Posted stock transfer cannot be deleted.");

        transfer.IsActive = false;
        transfer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PostAsync(int id)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var transfer = await _context.StockTransfers
                .Include(x => x.StockTransferLines)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsActive);

            if (transfer == null)
                return false;

            if (transfer.IsPosted)
                throw new InvalidOperationException(
                    "Stock transfer is already posted.");

            if (transfer.StockTransferLines.Count == 0)
                throw new InvalidOperationException(
                    "Stock transfer has no lines.");

            ValidateTransferLocations(transfer);

            foreach (var line in transfer.StockTransferLines
                         .OrderBy(x => x.LineNumber))
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(x =>
                        x.Id == line.ProductId &&
                        x.IsActive);

                if (product == null)
                    throw new InvalidOperationException(
                        $"Product {line.ProductId} not found or inactive.");

                // SOURCE BALANCE
                var sourceBalance =
                    await _context.StockBalances
                        .FirstOrDefaultAsync(x =>
                            x.ProductId == line.ProductId &&
                            x.WarehouseId == transfer.FromWarehouseId &&
                            x.BranchId == transfer.FromBranchId &&
                            x.IsActive);

                if (sourceBalance == null)
                {
                    throw new InvalidOperationException(
                        $"Source stock balance not found for Product {line.ProductId}.");
                }

                if (line.Quantity >
                    sourceBalance.AvailableQuantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for Product {line.ProductId}. " +
                        $"Available: {sourceBalance.AvailableQuantity}, " +
                        $"Requested: {line.Quantity}.");
                }

                // Transfer cost should come from existing stock.
                var transferUnitCost =
                    sourceBalance.AverageCost;

                // SOURCE OUT
                sourceBalance.Quantity -= line.Quantity;

                sourceBalance.AvailableQuantity =
                    sourceBalance.Quantity -
                    sourceBalance.ReservedQuantity;

                sourceBalance.StockValue =
                    sourceBalance.Quantity *
                    sourceBalance.AverageCost;

                var sourceTransaction =
                    new StockTransaction
                    {
                        ProductId = line.ProductId,

                        WarehouseId =
                            transfer.FromWarehouseId,

                        BranchId =
                            transfer.FromBranchId,

                        FiscalYearId =
                            transfer.FiscalYearId,

                        FiscalYearPeriodId =
                            transfer.FiscalYearPeriodId,

                        TransactionDate =
                            transfer.TransferDate,

                        TransactionType =
                            "TransferOut",

                        ReferenceType =
                            "StockTransfer",

                        ReferenceId =
                            transfer.Id,

                        ReferenceNumber =
                            transfer.TransferNumber,

                        QuantityIn = 0,

                        QuantityOut =
                            line.Quantity,

                        BalanceQuantity =
                            sourceBalance.Quantity,

                        UnitCost =
                            transferUnitCost,

                        TotalCost =
                            line.Quantity *
                            transferUnitCost,

                        Description =
                            line.Description ??
                            transfer.Reason,

                        IsActive = true,

                        CreatedAt =
                            DateTime.UtcNow
                    };

                _context.StockTransactions.Add(
                    sourceTransaction);

                // DESTINATION BALANCE
                var destinationBalance =
                    await _context.StockBalances
                        .FirstOrDefaultAsync(x =>
                            x.ProductId == line.ProductId &&
                            x.WarehouseId == transfer.ToWarehouseId &&
                            x.BranchId == transfer.ToBranchId &&
                            x.IsActive);

                if (destinationBalance == null)
                {
                    destinationBalance =
                        new StockBalance
                        {
                            ProductId =
                                line.ProductId,

                            WarehouseId =
                                transfer.ToWarehouseId,

                            BranchId =
                                transfer.ToBranchId,

                            Quantity = 0,

                            ReservedQuantity = 0,

                            AvailableQuantity = 0,

                            AverageCost =
                                transferUnitCost,

                            StockValue = 0,

                            IsActive = true,

                            CreatedAt =
                                DateTime.UtcNow
                        };

                    _context.StockBalances.Add(
                        destinationBalance);

                    await _context.SaveChangesAsync();
                }

                var oldDestinationQuantity =
                    destinationBalance.Quantity;

                var oldDestinationAverage =
                    destinationBalance.AverageCost;

                var newDestinationQuantity =
                    oldDestinationQuantity +
                    line.Quantity;

                if (newDestinationQuantity > 0)
                {
                    destinationBalance.AverageCost =
                        (
                            (oldDestinationQuantity *
                             oldDestinationAverage)
                            +
                            (line.Quantity *
                             transferUnitCost)
                        )
                        /
                        newDestinationQuantity;
                }

                destinationBalance.Quantity =
                    newDestinationQuantity;

                destinationBalance.AvailableQuantity =
                    destinationBalance.Quantity -
                    destinationBalance.ReservedQuantity;

                destinationBalance.StockValue =
                    destinationBalance.Quantity *
                    destinationBalance.AverageCost;

                // DESTINATION IN
                var destinationTransaction =
                    new StockTransaction
                    {
                        ProductId =
                            line.ProductId,

                        WarehouseId =
                            transfer.ToWarehouseId,

                        BranchId =
                            transfer.ToBranchId,

                        FiscalYearId =
                            transfer.FiscalYearId,

                        FiscalYearPeriodId =
                            transfer.FiscalYearPeriodId,

                        TransactionDate =
                            transfer.TransferDate,

                        TransactionType =
                            "TransferIn",

                        ReferenceType =
                            "StockTransfer",

                        ReferenceId =
                            transfer.Id,

                        ReferenceNumber =
                            transfer.TransferNumber,

                        QuantityIn =
                            line.Quantity,

                        QuantityOut = 0,

                        BalanceQuantity =
                            destinationBalance.Quantity,

                        UnitCost =
                            transferUnitCost,

                        TotalCost =
                            line.Quantity *
                            transferUnitCost,

                        Description =
                            line.Description ??
                            transfer.Reason,

                        IsActive = true,

                        CreatedAt =
                            DateTime.UtcNow
                    };

                _context.StockTransactions.Add(
                    destinationTransaction);
            }

            transfer.Status = "Posted";
            transfer.IsPosted = true;
            transfer.PostedAt = DateTime.UtcNow;
            transfer.UpdatedAt = DateTime.UtcNow;

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
        StockTransferDto dto)
    {
        if (dto.FiscalYearId <= 0)
            throw new ArgumentException(
                "FiscalYearId must be greater than zero.");

        if (dto.FiscalYearPeriodId <= 0)
            throw new ArgumentException(
                "FiscalYearPeriodId must be greater than zero.");

        if (string.IsNullOrWhiteSpace(dto.TransferNumber))
            throw new ArgumentException(
                "TransferNumber is required.");

        if (dto.TransferDate == default)
            throw new ArgumentException(
                "TransferDate is required.");
    }

    private static void ValidateTransferLocations(
        StockTransfer transfer)
    {
        if (transfer.FromWarehouseId == null &&
            transfer.FromBranchId == null)
        {
            throw new InvalidOperationException(
                "Source warehouse or branch is required.");
        }

        if (transfer.ToWarehouseId == null &&
            transfer.ToBranchId == null)
        {
            throw new InvalidOperationException(
                "Destination warehouse or branch is required.");
        }

        if (transfer.FromWarehouseId ==
            transfer.ToWarehouseId &&
            transfer.FromBranchId ==
            transfer.ToBranchId)
        {
            throw new InvalidOperationException(
                "Source and destination cannot be the same.");
        }
    }

    private static StockTransferDto MapToDto(
        StockTransfer transfer)
    {
        return new StockTransferDto
        {
            Id = transfer.Id,

            FiscalYearId =
                transfer.FiscalYearId,

            FiscalYearPeriodId =
                transfer.FiscalYearPeriodId,

            FromWarehouseId =
                transfer.FromWarehouseId,

            ToWarehouseId =
                transfer.ToWarehouseId,

            FromBranchId =
                transfer.FromBranchId,

            ToBranchId =
                transfer.ToBranchId,

            TransferNumber =
                transfer.TransferNumber,

            TransferDate =
                transfer.TransferDate,

            ReferenceNumber =
                transfer.ReferenceNumber,

            Reason =
                transfer.Reason,

            TotalQuantity =
                transfer.TotalQuantity,

            TotalValue =
                transfer.TotalValue,

            Status =
                transfer.Status,

            IsPosted =
                transfer.IsPosted,

            PostedAt =
                transfer.PostedAt,

            IsActive =
                transfer.IsActive,

            CreatedAt =
                transfer.CreatedAt,

            UpdatedAt =
                transfer.UpdatedAt,

            Lines =
                transfer.StockTransferLines
                    .OrderBy(x => x.LineNumber)
                    .Select(x =>
                        new StockTransferLineDto
                        {
                            Id = x.Id,

                            StockTransferId =
                                x.StockTransferId,

                            ProductId =
                                x.ProductId,

                            Quantity =
                                x.Quantity,

                            UnitCost =
                                x.UnitCost,

                            LineTotal =
                                x.LineTotal,

                            Description =
                                x.Description,

                            LineNumber =
                                x.LineNumber
                        })
                    .ToList()
        };
    }
}