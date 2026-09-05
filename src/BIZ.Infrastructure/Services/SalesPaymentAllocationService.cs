using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class SalesPaymentAllocationService
    : ISalesPaymentAllocationService
{
    private readonly TenantDbContext _context;

    public SalesPaymentAllocationService(
        TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SalesPaymentAllocationDto>>
        GetAllAsync()
    {
        return await _context.SalesPaymentAllocations
            .OrderByDescending(x => x.Id)
            .Select(x => new SalesPaymentAllocationDto
            {
                Id = x.Id,
                SalesPaymentId = x.SalesPaymentId,
                SalesInvoiceId = x.SalesInvoiceId,
                AllocatedAmount = x.AllocatedAmount,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<SalesPaymentAllocationDto?>
        GetByIdAsync(int id)
    {
        return await _context.SalesPaymentAllocations
            .Where(x => x.Id == id)
            .Select(x => new SalesPaymentAllocationDto
            {
                Id = x.Id,
                SalesPaymentId = x.SalesPaymentId,
                SalesInvoiceId = x.SalesInvoiceId,
                AllocatedAmount = x.AllocatedAmount,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SalesPaymentAllocationDto>
        CreateAsync(SalesPaymentAllocationDto dto)
    {
        if (dto.SalesPaymentId <= 0)
            throw new Exception("SalesPaymentId is required.");

        if (dto.SalesInvoiceId <= 0)
            throw new Exception("SalesInvoiceId is required.");

        if (dto.AllocatedAmount <= 0)
            throw new Exception(
                "Allocated amount must be greater than zero.");

        var payment = await _context.SalesPayments
            .FirstOrDefaultAsync(x =>
                x.Id == dto.SalesPaymentId &&
                x.IsActive);

        if (payment == null)
            throw new Exception("Sales payment not found.");

        if (payment.Status != "Draft")
            throw new Exception(
                "Allocation can only be added to Draft payment.");

        var invoice = await _context.SalesInvoices
            .FirstOrDefaultAsync(x =>
                x.Id == dto.SalesInvoiceId &&
                x.IsActive);

        if (invoice == null)
            throw new Exception("Sales invoice not found.");

        if (invoice.CustomerId != payment.CustomerId)
            throw new Exception(
                "Invoice customer does not match payment customer.");

        var duplicate = await _context
            .SalesPaymentAllocations
            .AnyAsync(x =>
                x.SalesPaymentId == dto.SalesPaymentId &&
                x.SalesInvoiceId == dto.SalesInvoiceId);

        if (duplicate)
            throw new Exception(
                "This invoice is already allocated to this payment.");

        var availableBalance =
            invoice.GrandTotal - invoice.PaidAmount;

        if (dto.AllocatedAmount > availableBalance)
            throw new Exception(
                "Allocated amount exceeds invoice balance.");

        var currentAllocationTotal =
            await _context.SalesPaymentAllocations
                .Where(x =>
                    x.SalesPaymentId == dto.SalesPaymentId)
                .SumAsync(x => (decimal?)x.AllocatedAmount) ?? 0;

        if (currentAllocationTotal + dto.AllocatedAmount
            > payment.Amount)
        {
            throw new Exception(
                "Allocation total cannot exceed payment amount.");
        }

        var allocation = new SalesPaymentAllocation
        {
            SalesPaymentId = dto.SalesPaymentId,
            SalesInvoiceId = dto.SalesInvoiceId,
            AllocatedAmount = dto.AllocatedAmount,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.SalesPaymentAllocations.Add(allocation);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(allocation.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        SalesPaymentAllocationDto dto)
    {
        var allocation =
            await _context.SalesPaymentAllocations
                .Include(x => x.SalesPayment)
                .FirstOrDefaultAsync(x => x.Id == id);

        if (allocation == null)
            return false;

        if (allocation.SalesPayment.Status != "Draft")
            throw new Exception(
                "Only Draft payment allocation can be updated.");

        if (dto.AllocatedAmount <= 0)
            throw new Exception(
                "Allocated amount must be greater than zero.");

        allocation.AllocatedAmount = dto.AllocatedAmount;
        allocation.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var allocation =
            await _context.SalesPaymentAllocations
                .Include(x => x.SalesPayment)
                .FirstOrDefaultAsync(x => x.Id == id);

        if (allocation == null)
            return false;

        if (allocation.SalesPayment.Status != "Draft")
            throw new Exception(
                "Only Draft payment allocation can be deleted.");

        _context.SalesPaymentAllocations.Remove(allocation);

        await _context.SaveChangesAsync();

        return true;
    }
}