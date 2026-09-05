using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class PurchasePaymentAllocationService
    : IPurchasePaymentAllocationService
{
    private readonly TenantDbContext _context;

    public PurchasePaymentAllocationService(
        TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PurchasePaymentAllocationDto>>
        GetAllAsync()
    {
        return await _context.PurchasePaymentAllocations
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.PurchasePayment.IsActive &&
                x.PurchaseInvoice.IsActive)
            .OrderByDescending(x => x.Id)
            .Select(x => new PurchasePaymentAllocationDto
            {
                Id = x.Id,
                PurchasePaymentId = x.PurchasePaymentId,
                PurchaseInvoiceId = x.PurchaseInvoiceId,
                AllocatedAmount = x.AllocatedAmount,
                Notes = x.Notes,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<PurchasePaymentAllocationDto?>
        GetByIdAsync(int id)
    {
        return await _context.PurchasePaymentAllocations
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.IsActive &&
                x.PurchasePayment.IsActive &&
                x.PurchaseInvoice.IsActive)
            .Select(x => new PurchasePaymentAllocationDto
            {
                Id = x.Id,
                PurchasePaymentId = x.PurchasePaymentId,
                PurchaseInvoiceId = x.PurchaseInvoiceId,
                AllocatedAmount = x.AllocatedAmount,
                Notes = x.Notes,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PurchasePaymentAllocationDto> CreateAsync(
        PurchasePaymentAllocationDto dto)
    {
        if (dto.PurchasePaymentId <= 0)
            throw new ArgumentException(
                "PurchasePaymentId is required.");

        if (dto.PurchaseInvoiceId <= 0)
            throw new ArgumentException(
                "PurchaseInvoiceId is required.");

        if (dto.AllocatedAmount <= 0)
            throw new ArgumentException(
                "AllocatedAmount must be greater than zero.");

        var payment = await _context.PurchasePayments
            .FirstOrDefaultAsync(x =>
                x.Id == dto.PurchasePaymentId &&
                x.IsActive);

        if (payment == null)
            throw new ArgumentException(
                "Purchase payment not found or inactive.");

        if (payment.IsPosted)
            throw new InvalidOperationException(
                "Posted payment allocation cannot be changed.");

        var invoice = await _context.PurchaseInvoices
            .FirstOrDefaultAsync(x =>
                x.Id == dto.PurchaseInvoiceId &&
                x.IsActive);

        if (invoice == null)
            throw new ArgumentException(
                "Purchase invoice not found or inactive.");

        if (invoice.SupplierId != payment.SupplierId)
            throw new ArgumentException(
                "Purchase invoice does not belong to payment supplier.");

        var duplicate = await _context
            .PurchasePaymentAllocations
            .AnyAsync(x =>
                x.PurchasePaymentId ==
                    dto.PurchasePaymentId &&
                x.PurchaseInvoiceId ==
                    dto.PurchaseInvoiceId &&
                x.IsActive);

        if (duplicate)
            throw new ArgumentException(
                "This invoice is already allocated to this payment.");

        var existingAllocationTotal = await _context
            .PurchasePaymentAllocations
            .Where(x =>
                x.PurchasePaymentId ==
                    dto.PurchasePaymentId &&
                x.IsActive)
            .SumAsync(x =>
                (decimal?)x.AllocatedAmount) ?? 0;

        if (existingAllocationTotal +
            dto.AllocatedAmount >
            payment.Amount)
        {
            throw new ArgumentException(
                "Total allocations cannot exceed payment amount.");
        }

        var invoiceTotal = await _context
            .PurchaseInvoiceLines
            .Where(x =>
                x.PurchaseInvoiceId ==
                    dto.PurchaseInvoiceId)
            .SumAsync(x =>
                (decimal?)x.LineTotal) ?? 0;

        var invoiceAllocated = await _context
            .PurchasePaymentAllocations
            .Where(x =>
                x.PurchaseInvoiceId ==
                    dto.PurchaseInvoiceId &&
                x.IsActive)
            .SumAsync(x =>
                (decimal?)x.AllocatedAmount) ?? 0;

        var remaining = invoiceTotal - invoiceAllocated;

        if (dto.AllocatedAmount > remaining)
        {
            throw new ArgumentException(
                $"Allocation exceeds invoice remaining balance {remaining}.");
        }

        var allocation = new PurchasePaymentAllocation
        {
            PurchasePaymentId = dto.PurchasePaymentId,
            PurchaseInvoiceId = dto.PurchaseInvoiceId,
            AllocatedAmount = dto.AllocatedAmount,
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.PurchasePaymentAllocations.Add(allocation);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(allocation.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        PurchasePaymentAllocationDto dto)
    {
        var allocation = await _context
            .PurchasePaymentAllocations
            .Include(x => x.PurchasePayment)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (allocation == null)
            return false;

        if (allocation.PurchasePayment.IsPosted)
            throw new InvalidOperationException(
                "Posted payment allocation cannot be updated.");

        if (dto.PurchasePaymentId !=
            allocation.PurchasePaymentId)
        {
            throw new ArgumentException(
                "PurchasePaymentId cannot be changed.");
        }

        if (dto.PurchaseInvoiceId !=
            allocation.PurchaseInvoiceId)
        {
            throw new ArgumentException(
                "PurchaseInvoiceId cannot be changed.");
        }

        if (dto.AllocatedAmount <= 0)
            throw new ArgumentException(
                "AllocatedAmount must be greater than zero.");

        var otherAllocationTotal = await _context
            .PurchasePaymentAllocations
            .Where(x =>
                x.PurchasePaymentId ==
                    allocation.PurchasePaymentId &&
                x.Id != id &&
                x.IsActive)
            .SumAsync(x =>
                (decimal?)x.AllocatedAmount) ?? 0;

        if (otherAllocationTotal +
            dto.AllocatedAmount >
            allocation.PurchasePayment.Amount)
        {
            throw new ArgumentException(
                "Total allocations cannot exceed payment amount.");
        }

        allocation.AllocatedAmount =
            dto.AllocatedAmount;

        allocation.Notes = dto.Notes;

        allocation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var allocation = await _context
            .PurchasePaymentAllocations
            .Include(x => x.PurchasePayment)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (allocation == null)
            return false;

        if (allocation.PurchasePayment.IsPosted)
            throw new InvalidOperationException(
                "Posted payment allocation cannot be deleted.");

        allocation.IsActive = false;
        allocation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}