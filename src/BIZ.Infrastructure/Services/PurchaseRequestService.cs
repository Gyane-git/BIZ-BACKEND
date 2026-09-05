using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class PurchaseRequestService : IPurchaseRequestService
{
    private readonly TenantDbContext _context;

    public PurchaseRequestService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PurchaseRequestDto>> GetAllAsync()
    {
        return await _context.PurchaseRequests
            .Include(x => x.PurchaseRequestLines)
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Id)
            .Select(x => new PurchaseRequestDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                RequestNumber = x.RequestNumber,
                RequestDate = x.RequestDate,
                RequiredByDate = x.RequiredByDate,
                Priority = x.Priority,
                Status = x.Status,
                Purpose = x.Purpose,
                Notes = x.Notes,
                BranchId = x.BranchId,
                WarehouseId = x.WarehouseId,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,

                Lines = x.PurchaseRequestLines
                    .OrderBy(l => l.LineNumber)
                    .Select(l => new PurchaseRequestLineDto
                    {
                        Id = l.Id,
                        PurchaseRequestId = l.PurchaseRequestId,
                        ProductId = l.ProductId,
                        UnitId = l.UnitId,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        LineNumber = l.LineNumber,
                        Notes = l.Notes
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<PurchaseRequestDto?> GetByIdAsync(int id)
    {
        return await _context.PurchaseRequests
            .Include(x => x.PurchaseRequestLines)
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new PurchaseRequestDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                RequestNumber = x.RequestNumber,
                RequestDate = x.RequestDate,
                RequiredByDate = x.RequiredByDate,
                Priority = x.Priority,
                Status = x.Status,
                Purpose = x.Purpose,
                Notes = x.Notes,
                BranchId = x.BranchId,
                WarehouseId = x.WarehouseId,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,

                Lines = x.PurchaseRequestLines
                    .OrderBy(l => l.LineNumber)
                    .Select(l => new PurchaseRequestLineDto
                    {
                        Id = l.Id,
                        PurchaseRequestId = l.PurchaseRequestId,
                        ProductId = l.ProductId,
                        UnitId = l.UnitId,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        LineNumber = l.LineNumber,
                        Notes = l.Notes
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PurchaseRequestDto> CreateAsync(
        PurchaseRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RequestNumber))
            throw new Exception("Request number is required.");

        if (dto.FiscalYearId <= 0)
            throw new Exception("FiscalYearId is required.");

        if (dto.FiscalYearPeriodId <= 0)
            throw new Exception("FiscalYearPeriodId is required.");

        if (dto.RequestDate == default)
            throw new Exception("Request date is required.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new Exception(
                "At least one purchase request line is required.");

        var requestNumber = dto.RequestNumber.Trim().ToUpper();

        var duplicate = await _context.PurchaseRequests
            .AnyAsync(x => x.RequestNumber == requestNumber);

        if (duplicate)
            throw new Exception(
                "Purchase request number already exists.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new Exception("Fiscal year not found.");

        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearPeriodId &&
                x.IsActive);

        if (period == null)
            throw new Exception(
                "Fiscal year period not found.");

        if (period.FiscalYearId != dto.FiscalYearId)
            throw new Exception(
                "Fiscal year period does not belong to fiscal year.");

        if (dto.RequestDate.Date < period.StartDate.Date ||
            dto.RequestDate.Date > period.EndDate.Date)
        {
            throw new Exception(
                "Request date is outside fiscal year period.");
        }

        var priority = dto.Priority.Trim();

        var allowedPriorities = new[]
        {
            "Low",
            "Normal",
            "High",
            "Urgent"
        };

        if (!allowedPriorities.Contains(
                priority,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new Exception(
                "Priority must be Low, Normal, High or Urgent.");
        }

        priority = allowedPriorities.First(
            x => x.Equals(
                priority,
                StringComparison.OrdinalIgnoreCase));

        var lineNumbers = dto.Lines
            .Select(x => x.LineNumber)
            .ToList();

        if (lineNumbers.Any(x => x <= 0))
            throw new Exception(
                "LineNumber must be greater than zero.");

        if (lineNumbers.Distinct().Count() != lineNumbers.Count)
            throw new Exception(
                "Duplicate line number is not allowed.");

        foreach (var line in dto.Lines)
        {
            if (line.ProductId <= 0)
                throw new Exception(
                    "ProductId is required for every line.");

            if (line.Quantity <= 0)
                throw new Exception(
                    "Quantity must be greater than zero.");

            var productExists = await _context.Products
                .AnyAsync(x =>
                    x.Id == line.ProductId &&
                    x.IsActive);

            if (!productExists)
                throw new Exception(
                    $"Product {line.ProductId} not found.");
        }

        var request = new PurchaseRequest
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,
            RequestNumber = requestNumber,
            RequestDate = dto.RequestDate,
            RequiredByDate = dto.RequiredByDate,
            Priority = priority,
            Status = "Draft",
            Purpose = dto.Purpose,
            Notes = dto.Notes,
            BranchId = dto.BranchId,
            WarehouseId = dto.WarehouseId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var line in dto.Lines)
        {
            request.PurchaseRequestLines.Add(
                new PurchaseRequestLine
                {
                    ProductId = line.ProductId,
                    UnitId = line.UnitId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    LineNumber = line.LineNumber,
                    Notes = line.Notes
                });
        }

        _context.PurchaseRequests.Add(request);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(request.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        PurchaseRequestDto dto)
    {
        var request = await _context.PurchaseRequests
            .Include(x => x.PurchaseRequestLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (request == null)
            return false;

        if (request.Status != "Draft")
            throw new Exception(
                "Only Draft purchase request can be updated.");

        if (string.IsNullOrWhiteSpace(dto.RequestNumber))
            throw new Exception(
                "Request number is required.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new Exception(
                "At least one purchase request line is required.");

        var requestNumber = dto.RequestNumber
            .Trim()
            .ToUpper();

        var duplicate = await _context.PurchaseRequests
            .AnyAsync(x =>
                x.Id != id &&
                x.RequestNumber == requestNumber);

        if (duplicate)
            throw new Exception(
                "Purchase request number already exists.");

        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearPeriodId &&
                x.IsActive);

        if (period == null)
            throw new Exception(
                "Fiscal year period not found.");

        if (period.FiscalYearId != dto.FiscalYearId)
            throw new Exception(
                "Fiscal year period does not belong to fiscal year.");

        if (dto.RequestDate.Date < period.StartDate.Date ||
            dto.RequestDate.Date > period.EndDate.Date)
        {
            throw new Exception(
                "Request date is outside fiscal year period.");
        }

        var lineNumbers = dto.Lines
            .Select(x => x.LineNumber)
            .ToList();

        if (lineNumbers.Any(x => x <= 0))
            throw new Exception(
                "LineNumber must be greater than zero.");

        if (lineNumbers.Distinct().Count() != lineNumbers.Count)
            throw new Exception(
                "Duplicate line number is not allowed.");

        foreach (var line in dto.Lines)
        {
            if (line.ProductId <= 0)
                throw new Exception(
                    "ProductId is required.");

            if (line.Quantity <= 0)
                throw new Exception(
                    "Quantity must be greater than zero.");

            var productExists = await _context.Products
                .AnyAsync(x =>
                    x.Id == line.ProductId &&
                    x.IsActive);

            if (!productExists)
                throw new Exception(
                    $"Product {line.ProductId} not found.");
        }

        request.FiscalYearId = dto.FiscalYearId;
        request.FiscalYearPeriodId = dto.FiscalYearPeriodId;
        request.RequestNumber = requestNumber;
        request.RequestDate = dto.RequestDate;
        request.RequiredByDate = dto.RequiredByDate;
        request.Priority = dto.Priority.Trim();
        request.Purpose = dto.Purpose;
        request.Notes = dto.Notes;
        request.BranchId = dto.BranchId;
        request.WarehouseId = dto.WarehouseId;
        request.UpdatedAt = DateTime.UtcNow;

        _context.PurchaseRequestLines.RemoveRange(
            request.PurchaseRequestLines);

        foreach (var line in dto.Lines)
        {
            request.PurchaseRequestLines.Add(
                new PurchaseRequestLine
                {
                    ProductId = line.ProductId,
                    UnitId = line.UnitId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    LineNumber = line.LineNumber,
                    Notes = line.Notes
                });
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var request = await _context.PurchaseRequests
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (request == null)
            return false;

        if (request.Status != "Draft")
            throw new Exception(
                "Only Draft purchase request can be deleted.");

        request.IsActive = false;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}