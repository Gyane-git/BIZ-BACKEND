using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class DeliveryChallanService : IDeliveryChallanService
{
    private readonly TenantDbContext _context;

    public DeliveryChallanService(TenantDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // GET ALL
    // =========================================================

    public async Task<IEnumerable<DeliveryChallanDto>> GetAllAsync()
    {
        var challans = await _context.DeliveryChallans
            .Include(x => x.DeliveryChallanLines)
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return challans.Select(MapToDto);
    }

    // =========================================================
    // GET BY ID
    // =========================================================

    public async Task<DeliveryChallanDto?> GetByIdAsync(int id)
    {
        var challan = await _context.DeliveryChallans
            .Include(x => x.DeliveryChallanLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (challan == null)
            return null;

        return MapToDto(challan);
    }

    // =========================================================
    // CREATE
    // =========================================================

    public async Task<DeliveryChallanDto> CreateAsync(
        DeliveryChallanDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ChallanNumber))
            throw new ArgumentException(
                "Challan number is required.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException(
                "At least one delivery challan line is required.");

        // -----------------------------------------------------
        // Duplicate number
        // -----------------------------------------------------

        var exists = await _context.DeliveryChallans
            .AnyAsync(x =>
                x.ChallanNumber == dto.ChallanNumber);

        if (exists)
            throw new ArgumentException(
                $"Challan number '{dto.ChallanNumber}' already exists.");

        // -----------------------------------------------------
        // Fiscal Year
        // -----------------------------------------------------

        var fiscalYearExists =
            await _context.FiscalYears
                .AnyAsync(x => x.Id == dto.FiscalYearId);

        if (!fiscalYearExists)
            throw new ArgumentException(
                "Fiscal year not found.");

        // -----------------------------------------------------
        // Fiscal Period
        // -----------------------------------------------------

        var fiscalPeriod =
            await _context.FiscalYearPeriods
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.FiscalYearPeriodId &&
                    x.FiscalYearId == dto.FiscalYearId);

        if (fiscalPeriod == null)
            throw new ArgumentException(
                "Fiscal year period not found or does not belong to the selected fiscal year.");

        // -----------------------------------------------------
        // Date validation
        // -----------------------------------------------------

        if (dto.ChallanDate.Date <
                fiscalPeriod.StartDate.Date ||
            dto.ChallanDate.Date >
                fiscalPeriod.EndDate.Date)
        {
            throw new ArgumentException(
                "Challan date must be within the selected fiscal year period.");
        }

        // -----------------------------------------------------
        // Create lines
        // -----------------------------------------------------

        var lines = new List<DeliveryChallanLine>();

        foreach (var lineDto in
                 dto.Lines.OrderBy(x => x.LineNumber))
        {
            ValidateLine(lineDto);

            var duplicateLine =
                lines.Any(x =>
                    x.LineNumber == lineDto.LineNumber);

            if (duplicateLine)
            {
                throw new ArgumentException(
                    $"Line number {lineDto.LineNumber} is duplicated.");
            }

            lines.Add(new DeliveryChallanLine
            {
                ProductId = lineDto.ProductId,

                UnitId = lineDto.UnitId,

                Description = lineDto.Description,

                Quantity = lineDto.Quantity,

                LineNumber = lineDto.LineNumber
            });
        }

        // -----------------------------------------------------
        // Create challan
        // -----------------------------------------------------

        var challan = new DeliveryChallan
        {
            FiscalYearId =
                dto.FiscalYearId,

            FiscalYearPeriodId =
                dto.FiscalYearPeriodId,

            CustomerId =
                dto.CustomerId,

            SalesOrderId =
                dto.SalesOrderId,

            ChallanNumber =
                dto.ChallanNumber,

            ChallanDate =
                dto.ChallanDate,

            ExpectedReturnDate =
                dto.ExpectedReturnDate,

            Status =
                "Draft",

            VehicleNumber =
                dto.VehicleNumber,

            DriverName =
                dto.DriverName,

            DriverContact =
                dto.DriverContact,

            ReferenceNumber =
                dto.ReferenceNumber,

            Notes =
                dto.Notes,

            BranchId =
                dto.BranchId,

            WarehouseId =
                dto.WarehouseId,

            IsActive = true,

            CreatedAt =
                DateTime.UtcNow,

            DeliveryChallanLines =
                lines
        };

        _context.DeliveryChallans.Add(challan);

        await _context.SaveChangesAsync();

        return MapToDto(challan);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    public async Task<bool> UpdateAsync(
        int id,
        DeliveryChallanDto dto)
    {
        var challan =
            await _context.DeliveryChallans
                .Include(x => x.DeliveryChallanLines)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsActive);

        if (challan == null)
            return false;

        // -----------------------------------------------------
        // Only Draft can update
        // -----------------------------------------------------

        if (!string.Equals(
                challan.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only Draft delivery challans can be updated.");
        }

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException(
                "At least one delivery challan line is required.");

        // -----------------------------------------------------
        // Validate lines
        // -----------------------------------------------------

        var lineNumbers = new HashSet<int>();

        foreach (var line in dto.Lines)
        {
            ValidateLine(line);

            if (!lineNumbers.Add(line.LineNumber))
            {
                throw new ArgumentException(
                    $"Line number {line.LineNumber} is duplicated.");
            }
        }

        // -----------------------------------------------------
        // Update header
        // -----------------------------------------------------

        challan.CustomerId =
            dto.CustomerId;

        challan.SalesOrderId =
            dto.SalesOrderId;

        challan.ChallanDate =
            dto.ChallanDate;

        challan.ExpectedReturnDate =
            dto.ExpectedReturnDate;

        challan.Status =
            dto.Status;

        challan.VehicleNumber =
            dto.VehicleNumber;

        challan.DriverName =
            dto.DriverName;

        challan.DriverContact =
            dto.DriverContact;

        challan.ReferenceNumber =
            dto.ReferenceNumber;

        challan.Notes =
            dto.Notes;

        challan.BranchId =
            dto.BranchId;

        challan.WarehouseId =
            dto.WarehouseId;

        challan.UpdatedAt =
            DateTime.UtcNow;

        // -----------------------------------------------------
        // Replace lines
        // -----------------------------------------------------

        _context.DeliveryChallanLines.RemoveRange(
            challan.DeliveryChallanLines);

        foreach (var lineDto in
                 dto.Lines.OrderBy(x => x.LineNumber))
        {
            challan.DeliveryChallanLines.Add(
                new DeliveryChallanLine
                {
                    DeliveryChallanId =
                        challan.Id,

                    ProductId =
                        lineDto.ProductId,

                    UnitId =
                        lineDto.UnitId,

                    Description =
                        lineDto.Description,

                    Quantity =
                        lineDto.Quantity,

                    LineNumber =
                        lineDto.LineNumber
                });
        }

        await _context.SaveChangesAsync();

        return true;
    }

    // =========================================================
    // DELETE
    // =========================================================

    public async Task<bool> DeleteAsync(int id)
    {
        var challan =
            await _context.DeliveryChallans
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsActive);

        if (challan == null)
            return false;

        challan.IsActive = false;

        challan.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    // =========================================================
    // VALIDATE LINE
    // =========================================================

    private static void ValidateLine(
        DeliveryChallanLineDto line)
    {
        if (line.ProductId <= 0)
            throw new ArgumentException(
                $"Invalid ProductId on line {line.LineNumber}.");

        if (line.Quantity <= 0)
            throw new ArgumentException(
                $"Quantity must be greater than zero on line {line.LineNumber}.");

        if (line.LineNumber <= 0)
            throw new ArgumentException(
                "Line number must be greater than zero.");
    }

    // =========================================================
    // MAP ENTITY → DTO
    // =========================================================

    private static DeliveryChallanDto MapToDto(
        DeliveryChallan challan)
    {
        return new DeliveryChallanDto
        {
            Id =
                challan.Id,

            FiscalYearId =
                challan.FiscalYearId,

            FiscalYearPeriodId =
                challan.FiscalYearPeriodId,

            CustomerId =
                challan.CustomerId,

            SalesOrderId =
                challan.SalesOrderId,

            ChallanNumber =
                challan.ChallanNumber,

            ChallanDate =
                challan.ChallanDate,

            ExpectedReturnDate =
                challan.ExpectedReturnDate,

            Status =
                challan.Status,

            VehicleNumber =
                challan.VehicleNumber,

            DriverName =
                challan.DriverName,

            DriverContact =
                challan.DriverContact,

            ReferenceNumber =
                challan.ReferenceNumber,

            Notes =
                challan.Notes,

            BranchId =
                challan.BranchId,

            WarehouseId =
                challan.WarehouseId,

            IsActive =
                challan.IsActive,

            CreatedAt =
                challan.CreatedAt,

            UpdatedAt =
                challan.UpdatedAt,

            Lines =
                challan.DeliveryChallanLines
                    .OrderBy(x => x.LineNumber)
                    .Select(x =>
                        new DeliveryChallanLineDto
                        {
                            Id =
                                x.Id,

                            DeliveryChallanId =
                                x.DeliveryChallanId,

                            ProductId =
                                x.ProductId,

                            UnitId =
                                x.UnitId,

                            Description =
                                x.Description,

                            Quantity =
                                x.Quantity,

                            LineNumber =
                                x.LineNumber
                        })
                    .ToList()
        };
    }
}