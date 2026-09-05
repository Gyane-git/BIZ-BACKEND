using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class PurchaseRequestLineService
    : IPurchaseRequestLineService
{
    private readonly TenantDbContext _context;

    public PurchaseRequestLineService(
        TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PurchaseRequestLineDto>>
        GetAllAsync()
    {
        return await _context.PurchaseRequestLines
            .Include(x => x.PurchaseRequest)
            .Where(x => x.PurchaseRequest.IsActive)
            .OrderByDescending(x => x.Id)
            .Select(x => new PurchaseRequestLineDto
            {
                Id = x.Id,
                PurchaseRequestId = x.PurchaseRequestId,
                ProductId = x.ProductId,
                UnitId = x.UnitId,
                Description = x.Description,
                Quantity = x.Quantity,
                LineNumber = x.LineNumber,
                Notes = x.Notes
            })
            .ToListAsync();
    }

    public async Task<PurchaseRequestLineDto?>
        GetByIdAsync(int id)
    {
        return await _context.PurchaseRequestLines
            .Include(x => x.PurchaseRequest)
            .Where(x =>
                x.Id == id &&
                x.PurchaseRequest.IsActive)
            .Select(x => new PurchaseRequestLineDto
            {
                Id = x.Id,
                PurchaseRequestId = x.PurchaseRequestId,
                ProductId = x.ProductId,
                UnitId = x.UnitId,
                Description = x.Description,
                Quantity = x.Quantity,
                LineNumber = x.LineNumber,
                Notes = x.Notes
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PurchaseRequestLineDto>
        CreateAsync(PurchaseRequestLineDto dto)
    {
        if (dto.PurchaseRequestId <= 0)
            throw new Exception(
                "PurchaseRequestId is required.");

        if (dto.ProductId <= 0)
            throw new Exception(
                "ProductId is required.");

        if (dto.Quantity <= 0)
            throw new Exception(
                "Quantity must be greater than zero.");

        if (dto.LineNumber <= 0)
            throw new Exception(
                "LineNumber must be greater than zero.");

        var request = await _context.PurchaseRequests
            .FirstOrDefaultAsync(x =>
                x.Id == dto.PurchaseRequestId &&
                x.IsActive);

        if (request == null)
            throw new Exception(
                "Purchase request not found.");

        if (request.Status != "Draft")
            throw new Exception(
                "Line can only be added to Draft request.");

        var productExists = await _context.Products
            .AnyAsync(x =>
                x.Id == dto.ProductId &&
                x.IsActive);

        if (!productExists)
            throw new Exception("Product not found.");

        var duplicateLine = await _context
            .PurchaseRequestLines
            .AnyAsync(x =>
                x.PurchaseRequestId == dto.PurchaseRequestId &&
                x.LineNumber == dto.LineNumber);

        if (duplicateLine)
            throw new Exception(
                "Line number already exists.");

        var line = new PurchaseRequestLine
        {
            PurchaseRequestId = dto.PurchaseRequestId,
            ProductId = dto.ProductId,
            UnitId = dto.UnitId,
            Description = dto.Description,
            Quantity = dto.Quantity,
            LineNumber = dto.LineNumber,
            Notes = dto.Notes
        };

        _context.PurchaseRequestLines.Add(line);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(line.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        PurchaseRequestLineDto dto)
    {
        var line = await _context.PurchaseRequestLines
            .Include(x => x.PurchaseRequest)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        if (line.PurchaseRequest.Status != "Draft")
            throw new Exception(
                "Only Draft request line can be updated.");

        if (dto.ProductId <= 0)
            throw new Exception(
                "ProductId is required.");

        if (dto.Quantity <= 0)
            throw new Exception(
                "Quantity must be greater than zero.");

        if (dto.LineNumber <= 0)
            throw new Exception(
                "LineNumber must be greater than zero.");

        var productExists = await _context.Products
            .AnyAsync(x =>
                x.Id == dto.ProductId &&
                x.IsActive);

        if (!productExists)
            throw new Exception("Product not found.");

        var duplicateLine = await _context
            .PurchaseRequestLines
            .AnyAsync(x =>
                x.Id != id &&
                x.PurchaseRequestId ==
                    line.PurchaseRequestId &&
                x.LineNumber == dto.LineNumber);

        if (duplicateLine)
            throw new Exception(
                "Line number already exists.");

        line.ProductId = dto.ProductId;
        line.UnitId = dto.UnitId;
        line.Description = dto.Description;
        line.Quantity = dto.Quantity;
        line.LineNumber = dto.LineNumber;
        line.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var line = await _context.PurchaseRequestLines
            .Include(x => x.PurchaseRequest)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        if (line.PurchaseRequest.Status != "Draft")
            throw new Exception(
                "Only Draft request line can be deleted.");

        _context.PurchaseRequestLines.Remove(line);

        await _context.SaveChangesAsync();

        return true;
    }
}