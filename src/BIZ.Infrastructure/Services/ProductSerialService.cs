using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ProductSerialService : IProductSerialService
{
    private readonly TenantDbContext _db;

    private static readonly string[] AllowedStatuses =
    {
        "Available",
        "Sold",
        "Reserved",
        "Damaged",
        "Returned",
        "Service"
    };

    public ProductSerialService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProductSerialDto>> GetAllAsync()
    {
        return await _db.ProductSerials
            .AsNoTracking()
            .OrderBy(x => x.ProductId)
            .ThenBy(x => x.SerialNumber)
            .Select(x => new ProductSerialDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                ProductBatchId = x.ProductBatchId,
                SerialNumber = x.SerialNumber,
                PurchaseDate = x.PurchaseDate,
                WarrantyStartDate = x.WarrantyStartDate,
                WarrantyEndDate = x.WarrantyEndDate,
                PurchaseRate = x.PurchaseRate,
                SalesRate = x.SalesRate,
                Status = x.Status,
                Remarks = x.Remarks,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<ProductSerialDto>> GetByProductAsync(
        int productId)
    {
        return await _db.ProductSerials
            .AsNoTracking()
            .Where(x =>
                x.ProductId == productId &&
                x.IsActive)
            .OrderBy(x => x.SerialNumber)
            .Select(x => new ProductSerialDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                ProductBatchId = x.ProductBatchId,
                SerialNumber = x.SerialNumber,
                PurchaseDate = x.PurchaseDate,
                WarrantyStartDate = x.WarrantyStartDate,
                WarrantyEndDate = x.WarrantyEndDate,
                PurchaseRate = x.PurchaseRate,
                SalesRate = x.SalesRate,
                Status = x.Status,
                Remarks = x.Remarks,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<ProductSerialDto>> GetByBatchAsync(
        int productBatchId)
    {
        return await _db.ProductSerials
            .AsNoTracking()
            .Where(x =>
                x.ProductBatchId == productBatchId &&
                x.IsActive)
            .OrderBy(x => x.SerialNumber)
            .Select(x => new ProductSerialDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                ProductBatchId = x.ProductBatchId,
                SerialNumber = x.SerialNumber,
                PurchaseDate = x.PurchaseDate,
                WarrantyStartDate = x.WarrantyStartDate,
                WarrantyEndDate = x.WarrantyEndDate,
                PurchaseRate = x.PurchaseRate,
                SalesRate = x.SalesRate,
                Status = x.Status,
                Remarks = x.Remarks,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<ProductSerialDto?> GetByIdAsync(
        int id)
    {
        return await _db.ProductSerials
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductSerialDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                ProductBatchId = x.ProductBatchId,
                SerialNumber = x.SerialNumber,
                PurchaseDate = x.PurchaseDate,
                WarrantyStartDate = x.WarrantyStartDate,
                WarrantyEndDate = x.WarrantyEndDate,
                PurchaseRate = x.PurchaseRate,
                SalesRate = x.SalesRate,
                Status = x.Status,
                Remarks = x.Remarks,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductSerialDto?> GetBySerialNumberAsync(
        string serialNumber)
    {
        serialNumber = serialNumber.Trim();

        return await _db.ProductSerials
            .AsNoTracking()
            .Where(x =>
                x.SerialNumber == serialNumber)
            .Select(x => new ProductSerialDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                ProductBatchId = x.ProductBatchId,
                SerialNumber = x.SerialNumber,
                PurchaseDate = x.PurchaseDate,
                WarrantyStartDate = x.WarrantyStartDate,
                WarrantyEndDate = x.WarrantyEndDate,
                PurchaseRate = x.PurchaseRate,
                SalesRate = x.SalesRate,
                Status = x.Status,
                Remarks = x.Remarks,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductSerialDto> CreateAsync(
        ProductSerialDto dto)
    {
        dto.SerialNumber = dto.SerialNumber.Trim();
        dto.Status = dto.Status.Trim();
        dto.Remarks = dto.Remarks?.Trim();

        if (string.IsNullOrWhiteSpace(dto.SerialNumber))
        {
            throw new InvalidOperationException(
                "SerialNumber is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Status))
        {
            dto.Status = "Available";
        }

        ValidateStatus(dto.Status);

        var productExists = await _db.Products
            .AnyAsync(x =>
                x.Id == dto.ProductId &&
                x.IsActive);

        if (!productExists)
        {
            throw new InvalidOperationException(
                $"Product with ID {dto.ProductId} was not found or inactive.");
        }

        if (dto.ProductVariantId.HasValue)
        {
            var variantExists = await _db.ProductVariants
                .AnyAsync(x =>
                    x.Id == dto.ProductVariantId.Value &&
                    x.ProductId == dto.ProductId &&
                    x.IsActive);

            if (!variantExists)
            {
                throw new InvalidOperationException(
                    "ProductVariant was not found, inactive, or does not belong to the selected product.");
            }
        }

        if (dto.ProductBatchId.HasValue)
        {
            var batchExists = await _db.ProductBatches
                .AnyAsync(x =>
                    x.Id == dto.ProductBatchId.Value &&
                    x.ProductId == dto.ProductId &&
                    x.IsActive);

            if (!batchExists)
            {
                throw new InvalidOperationException(
                    "ProductBatch was not found, inactive, or does not belong to the selected product.");
            }
        }

        ValidateDates(dto);

        var serialExists = await _db.ProductSerials
            .AnyAsync(x =>
                x.SerialNumber == dto.SerialNumber);

        if (serialExists)
        {
            throw new InvalidOperationException(
                $"SerialNumber '{dto.SerialNumber}' already exists.");
        }

        ValidateRates(dto);

        var entity = new ProductSerial
        {
            ProductId = dto.ProductId,
            ProductVariantId = dto.ProductVariantId,
            ProductBatchId = dto.ProductBatchId,
            SerialNumber = dto.SerialNumber,
            PurchaseDate = dto.PurchaseDate,
            WarrantyStartDate = dto.WarrantyStartDate,
            WarrantyEndDate = dto.WarrantyEndDate,
            PurchaseRate = dto.PurchaseRate,
            SalesRate = dto.SalesRate,
            Status = dto.Status,
            Remarks = dto.Remarks,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.ProductSerials.Add(entity);

        await _db.SaveChangesAsync();

        dto.Id = entity.Id;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        ProductSerialDto dto)
    {
        dto.SerialNumber = dto.SerialNumber.Trim();
        dto.Status = dto.Status.Trim();
        dto.Remarks = dto.Remarks?.Trim();

        if (string.IsNullOrWhiteSpace(dto.SerialNumber))
        {
            throw new InvalidOperationException(
                "SerialNumber is required.");
        }

        ValidateStatus(dto.Status);

        var entity = await _db.ProductSerials
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
        {
            return false;
        }

        var productExists = await _db.Products
            .AnyAsync(x =>
                x.Id == dto.ProductId &&
                x.IsActive);

        if (!productExists)
        {
            throw new InvalidOperationException(
                $"Product with ID {dto.ProductId} was not found or inactive.");
        }

        if (dto.ProductVariantId.HasValue)
        {
            var variantExists = await _db.ProductVariants
                .AnyAsync(x =>
                    x.Id == dto.ProductVariantId.Value &&
                    x.ProductId == dto.ProductId &&
                    x.IsActive);

            if (!variantExists)
            {
                throw new InvalidOperationException(
                    "ProductVariant was not found, inactive, or does not belong to the selected product.");
            }
        }

        if (dto.ProductBatchId.HasValue)
        {
            var batchExists = await _db.ProductBatches
                .AnyAsync(x =>
                    x.Id == dto.ProductBatchId.Value &&
                    x.ProductId == dto.ProductId &&
                    x.IsActive);

            if (!batchExists)
            {
                throw new InvalidOperationException(
                    "ProductBatch was not found, inactive, or does not belong to the selected product.");
            }
        }

        ValidateDates(dto);

        var serialExists = await _db.ProductSerials
            .AnyAsync(x =>
                x.Id != id &&
                x.SerialNumber == dto.SerialNumber);

        if (serialExists)
        {
            throw new InvalidOperationException(
                $"SerialNumber '{dto.SerialNumber}' already exists.");
        }

        ValidateRates(dto);

        entity.ProductId = dto.ProductId;
        entity.ProductVariantId = dto.ProductVariantId;
        entity.ProductBatchId = dto.ProductBatchId;
        entity.SerialNumber = dto.SerialNumber;
        entity.PurchaseDate = dto.PurchaseDate;
        entity.WarrantyStartDate = dto.WarrantyStartDate;
        entity.WarrantyEndDate = dto.WarrantyEndDate;
        entity.PurchaseRate = dto.PurchaseRate;
        entity.SalesRate = dto.SalesRate;
        entity.Status = dto.Status;
        entity.Remarks = dto.Remarks;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.ProductSerials
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
        {
            return false;
        }

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    private static void ValidateStatus(string status)
    {
        if (!AllowedStatuses.Contains(
                status,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Invalid Status. Allowed values: Available, Sold, Reserved, Damaged, Returned, Service.");
        }
    }

    private static void ValidateDates(ProductSerialDto dto)
    {
        if (dto.WarrantyStartDate.HasValue &&
            dto.WarrantyEndDate.HasValue &&
            dto.WarrantyEndDate < dto.WarrantyStartDate)
        {
            throw new InvalidOperationException(
                "WarrantyEndDate cannot be earlier than WarrantyStartDate.");
        }
    }

    private static void ValidateRates(ProductSerialDto dto)
    {
        if (dto.PurchaseRate < 0)
        {
            throw new InvalidOperationException(
                "PurchaseRate cannot be negative.");
        }

        if (dto.SalesRate < 0)
        {
            throw new InvalidOperationException(
                "SalesRate cannot be negative.");
        }
    }
}