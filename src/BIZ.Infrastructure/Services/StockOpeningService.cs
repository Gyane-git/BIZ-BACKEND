using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class StockOpeningService : IStockOpeningService
{
    private readonly TenantDbContext _context;

    public StockOpeningService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StockOpeningDto>> GetAllAsync()
    {
        return await _context.StockOpenings
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.StockOpeningLines)
            .OrderByDescending(x => x.Id)
            .Select(x => new StockOpeningDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                WarehouseId = x.WarehouseId,
                BranchId = x.BranchId,
                OpeningNumber = x.OpeningNumber,
                OpeningDate = x.OpeningDate,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                TotalQuantity = x.TotalQuantity,
                TotalValue = x.TotalValue,
                Status = x.Status,
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Lines = x.StockOpeningLines
                    .OrderBy(l => l.LineNumber)
                    .Select(l => new StockOpeningLineDto
                    {
                        Id = l.Id,
                        StockOpeningId = l.StockOpeningId,
                        ProductId = l.ProductId,
                        Quantity = l.Quantity,
                        UnitCost = l.UnitCost,
                        TotalCost = l.TotalCost,
                        Description = l.Description,
                        LineNumber = l.LineNumber
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<StockOpeningDto?> GetByIdAsync(int id)
    {
        return await _context.StockOpenings
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Include(x => x.StockOpeningLines)
            .Select(x => new StockOpeningDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                WarehouseId = x.WarehouseId,
                BranchId = x.BranchId,
                OpeningNumber = x.OpeningNumber,
                OpeningDate = x.OpeningDate,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                TotalQuantity = x.TotalQuantity,
                TotalValue = x.TotalValue,
                Status = x.Status,
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Lines = x.StockOpeningLines
                    .OrderBy(l => l.LineNumber)
                    .Select(l => new StockOpeningLineDto
                    {
                        Id = l.Id,
                        StockOpeningId = l.StockOpeningId,
                        ProductId = l.ProductId,
                        Quantity = l.Quantity,
                        UnitCost = l.UnitCost,
                        TotalCost = l.TotalCost,
                        Description = l.Description,
                        LineNumber = l.LineNumber
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<StockOpeningDto> CreateAsync(StockOpeningDto dto)
    {
        if (dto.FiscalYearId <= 0)
            throw new ArgumentException("FiscalYearId is required.");

        if (dto.FiscalYearPeriodId <= 0)
            throw new ArgumentException("FiscalYearPeriodId is required.");

        if (string.IsNullOrWhiteSpace(dto.OpeningNumber))
            throw new ArgumentException("OpeningNumber is required.");

        var openingNumber = dto.OpeningNumber.Trim().ToUpperInvariant();

        if (await _context.StockOpenings
            .AnyAsync(x => x.OpeningNumber == openingNumber))
        {
            throw new InvalidOperationException(
                $"Opening number '{openingNumber}' already exists.");
        }

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new InvalidOperationException(
                "Fiscal year not found or inactive.");

        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearPeriodId &&
                x.IsActive);

        if (period == null)
            throw new InvalidOperationException(
                "Fiscal year period not found or inactive.");

        if (period.FiscalYearId != dto.FiscalYearId)
            throw new InvalidOperationException(
                "Fiscal year period does not belong to the selected fiscal year.");

        if (dto.OpeningDate.Date < period.StartDate.Date ||
            dto.OpeningDate.Date > period.EndDate.Date)
        {
            throw new InvalidOperationException(
                "Opening date must be within the selected fiscal year period.");
        }

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException(
                "At least one opening line is required.");

        var opening = new StockOpening
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,
            WarehouseId = dto.WarehouseId,
            BranchId = dto.BranchId,
            OpeningNumber = openingNumber,
            OpeningDate = dto.OpeningDate,
            ReferenceNumber = dto.ReferenceNumber?.Trim(),
            Description = dto.Description?.Trim(),
            Status = "Draft",
            IsPosted = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        decimal totalQuantity = 0;
        decimal totalValue = 0;

        var lineNumbers = new HashSet<int>();

        foreach (var lineDto in dto.Lines)
        {
            if (lineDto.ProductId <= 0)
                throw new ArgumentException(
                    "Valid ProductId is required.");

            if (lineDto.Quantity <= 0)
                throw new ArgumentException(
                    "Opening quantity must be greater than zero.");

            if (lineDto.UnitCost < 0)
                throw new ArgumentException(
                    "UnitCost cannot be negative.");

            if (!lineNumbers.Add(lineDto.LineNumber))
                throw new ArgumentException(
                    $"Duplicate LineNumber: {lineDto.LineNumber}");

            var productExists = await _context.Products
                .AnyAsync(x =>
                    x.Id == lineDto.ProductId &&
                    x.IsActive);

            if (!productExists)
                throw new InvalidOperationException(
                    $"Product ID {lineDto.ProductId} not found or inactive.");

            var totalCost = lineDto.Quantity * lineDto.UnitCost;

            opening.StockOpeningLines.Add(new StockOpeningLine
            {
                ProductId = lineDto.ProductId,
                Quantity = lineDto.Quantity,
                UnitCost = lineDto.UnitCost,
                TotalCost = totalCost,
                Description = lineDto.Description?.Trim(),
                LineNumber = lineDto.LineNumber
            });

            totalQuantity += lineDto.Quantity;
            totalValue += totalCost;
        }

        opening.TotalQuantity = totalQuantity;
        opening.TotalValue = totalValue;

        _context.StockOpenings.Add(opening);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(opening.Id))!;
    }

    public async Task<bool> UpdateAsync(int id, StockOpeningDto dto)
    {
        var opening = await _context.StockOpenings
            .Include(x => x.StockOpeningLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (opening == null)
            return false;

        if (opening.IsPosted)
            throw new InvalidOperationException(
                "Posted stock opening cannot be updated.");

        if (dto.FiscalYearId <= 0)
            throw new ArgumentException("FiscalYearId is required.");

        if (dto.FiscalYearPeriodId <= 0)
            throw new ArgumentException("FiscalYearPeriodId is required.");

        if (string.IsNullOrWhiteSpace(dto.OpeningNumber))
            throw new ArgumentException("OpeningNumber is required.");

        var openingNumber = dto.OpeningNumber.Trim().ToUpperInvariant();

        if (await _context.StockOpenings.AnyAsync(x =>
                x.Id != id &&
                x.OpeningNumber == openingNumber))
        {
            throw new InvalidOperationException(
                $"Opening number '{openingNumber}' already exists.");
        }

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new InvalidOperationException(
                "Fiscal year not found or inactive.");

        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearPeriodId &&
                x.IsActive);

        if (period == null)
            throw new InvalidOperationException(
                "Fiscal year period not found or inactive.");

        if (period.FiscalYearId != dto.FiscalYearId)
            throw new InvalidOperationException(
                "Fiscal year period does not belong to the selected fiscal year.");

        if (dto.OpeningDate.Date < period.StartDate.Date ||
            dto.OpeningDate.Date > period.EndDate.Date)
        {
            throw new InvalidOperationException(
                "Opening date must be within the selected fiscal year period.");
        }

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException(
                "At least one opening line is required.");

        opening.FiscalYearId = dto.FiscalYearId;
        opening.FiscalYearPeriodId = dto.FiscalYearPeriodId;
        opening.WarehouseId = dto.WarehouseId;
        opening.BranchId = dto.BranchId;
        opening.OpeningNumber = openingNumber;
        opening.OpeningDate = dto.OpeningDate;
        opening.ReferenceNumber = dto.ReferenceNumber?.Trim();
        opening.Description = dto.Description?.Trim();
        opening.UpdatedAt = DateTime.UtcNow;

        _context.StockOpeningLines.RemoveRange(
            opening.StockOpeningLines);

        opening.StockOpeningLines.Clear();

        decimal totalQuantity = 0;
        decimal totalValue = 0;

        var lineNumbers = new HashSet<int>();

        foreach (var lineDto in dto.Lines)
        {
            if (lineDto.ProductId <= 0)
                throw new ArgumentException(
                    "Valid ProductId is required.");

            if (lineDto.Quantity <= 0)
                throw new ArgumentException(
                    "Opening quantity must be greater than zero.");

            if (lineDto.UnitCost < 0)
                throw new ArgumentException(
                    "UnitCost cannot be negative.");

            if (!lineNumbers.Add(lineDto.LineNumber))
                throw new ArgumentException(
                    $"Duplicate LineNumber: {lineDto.LineNumber}");

            var productExists = await _context.Products
                .AnyAsync(x =>
                    x.Id == lineDto.ProductId &&
                    x.IsActive);

            if (!productExists)
                throw new InvalidOperationException(
                    $"Product ID {lineDto.ProductId} not found or inactive.");

            var totalCost = lineDto.Quantity * lineDto.UnitCost;

            opening.StockOpeningLines.Add(new StockOpeningLine
            {
                StockOpeningId = opening.Id,
                ProductId = lineDto.ProductId,
                Quantity = lineDto.Quantity,
                UnitCost = lineDto.UnitCost,
                TotalCost = totalCost,
                Description = lineDto.Description?.Trim(),
                LineNumber = lineDto.LineNumber
            });

            totalQuantity += lineDto.Quantity;
            totalValue += totalCost;
        }

        opening.TotalQuantity = totalQuantity;
        opening.TotalValue = totalValue;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var opening = await _context.StockOpenings
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (opening == null)
            return false;

        if (opening.IsPosted)
            throw new InvalidOperationException(
                "Posted stock opening cannot be deleted.");

        opening.IsActive = false;
        opening.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PostAsync(int id)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var opening = await _context.StockOpenings
                .Include(x => x.StockOpeningLines)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsActive);

            if (opening == null)
                return false;

            if (opening.IsPosted)
                throw new InvalidOperationException(
                    "Stock opening is already posted.");

            if (opening.StockOpeningLines.Count == 0)
                throw new InvalidOperationException(
                    "Stock opening must contain at least one line.");

            foreach (var line in opening.StockOpeningLines)
            {
                if (line.Quantity <= 0)
                    throw new InvalidOperationException(
                        $"Invalid quantity for Product ID {line.ProductId}.");

                if (line.UnitCost < 0)
                    throw new InvalidOperationException(
                        $"Invalid UnitCost for Product ID {line.ProductId}.");

                var productExists = await _context.Products
                    .AnyAsync(x =>
                        x.Id == line.ProductId &&
                        x.IsActive);

                if (!productExists)
                    throw new InvalidOperationException(
                        $"Product ID {line.ProductId} not found or inactive.");

                var balance = await _context.StockBalances
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == line.ProductId &&
                        x.WarehouseId == opening.WarehouseId &&
                        x.BranchId == opening.BranchId &&
                        x.IsActive);

                if (balance == null)
                {
                    balance = new StockBalance
                    {
                        ProductId = line.ProductId,
                        WarehouseId = opening.WarehouseId,
                        BranchId = opening.BranchId,
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

                var oldQuantity = balance.Quantity;
                var oldAverageCost = balance.AverageCost;

                var newQuantity =
                    oldQuantity + line.Quantity;

                decimal newAverageCost;

                if (newQuantity > 0)
                {
                    newAverageCost =
                        ((oldQuantity * oldAverageCost) +
                         (line.Quantity * line.UnitCost))
                        / newQuantity;
                }
                else
                {
                    newAverageCost = line.UnitCost;
                }

                balance.Quantity = newQuantity;

                balance.AvailableQuantity =
                    newQuantity - balance.ReservedQuantity;

                balance.AverageCost = newAverageCost;

                balance.StockValue =
                    newQuantity * newAverageCost;

                balance.UpdatedAt = DateTime.UtcNow;

                var stockTransaction = new StockTransaction
                {
                    ProductId = line.ProductId,
                    WarehouseId = opening.WarehouseId,
                    BranchId = opening.BranchId,
                    FiscalYearId = opening.FiscalYearId,
                    FiscalYearPeriodId = opening.FiscalYearPeriodId,
                    TransactionDate = opening.OpeningDate,
                    TransactionType = "Opening",
                    ReferenceType = "StockOpening",
                    ReferenceId = opening.Id,
                    ReferenceNumber = opening.OpeningNumber,
                    QuantityIn = line.Quantity,
                    QuantityOut = 0,
                    BalanceQuantity = newQuantity,
                    UnitCost = line.UnitCost,
                    TotalCost = line.TotalCost,
                    Description = line.Description ??
                                 opening.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.StockTransactions.Add(stockTransaction);
            }

            opening.Status = "Posted";
            opening.IsPosted = true;
            opening.PostedAt = DateTime.UtcNow;
            opening.UpdatedAt = DateTime.UtcNow;

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
}