using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Application.Services;

public class CreditNoteLineService : ICreditNoteLineService
{
    private readonly TenantDbContext _context;

    public CreditNoteLineService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<List<CreditNoteLineDto>> GetAllAsync()
    {
        return await _context.CreditNoteLines
            .AsNoTracking()
            .OrderBy(x => x.CreditNoteId)
            .ThenBy(x => x.LineNumber)
            .Select(x => new CreditNoteLineDto
            {
                Id = x.Id,
                CreditNoteId = x.CreditNoteId,
                ProductId = x.ProductId,
                Description = x.Description,
                Quantity = x.Quantity,
                Rate = x.Rate,
                DiscountAmount = x.DiscountAmount,
                TaxableAmount = x.TaxableAmount,
                TaxAmount = x.TaxAmount,
                LineTotal = x.LineTotal,
                LineNumber = x.LineNumber
            })
            .ToListAsync();
    }

    public async Task<List<CreditNoteLineDto>> GetByCreditNoteAsync(int creditNoteId)
    {
        return await _context.CreditNoteLines
            .AsNoTracking()
            .Where(x => x.CreditNoteId == creditNoteId)
            .OrderBy(x => x.LineNumber)
            .Select(x => new CreditNoteLineDto
            {
                Id = x.Id,
                CreditNoteId = x.CreditNoteId,
                ProductId = x.ProductId,
                Description = x.Description,
                Quantity = x.Quantity,
                Rate = x.Rate,
                DiscountAmount = x.DiscountAmount,
                TaxableAmount = x.TaxableAmount,
                TaxAmount = x.TaxAmount,
                LineTotal = x.LineTotal,
                LineNumber = x.LineNumber
            })
            .ToListAsync();
    }

    public async Task<CreditNoteLineDto?> GetByIdAsync(int id)
    {
        return await _context.CreditNoteLines
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CreditNoteLineDto
            {
                Id = x.Id,
                CreditNoteId = x.CreditNoteId,
                ProductId = x.ProductId,
                Description = x.Description,
                Quantity = x.Quantity,
                Rate = x.Rate,
                DiscountAmount = x.DiscountAmount,
                TaxableAmount = x.TaxableAmount,
                TaxAmount = x.TaxAmount,
                LineTotal = x.LineTotal,
                LineNumber = x.LineNumber
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CreditNoteLineDto> CreateAsync(CreditNoteLineDto dto)
    {
        if (dto.Quantity <= 0)
            throw new Exception("Quantity must be greater than zero.");

        if (dto.Rate < 0)
            throw new Exception("Rate cannot be negative.");

        if (dto.DiscountAmount < 0)
            throw new Exception("Discount amount cannot be negative.");

        if (dto.TaxAmount < 0)
            throw new Exception("Tax amount cannot be negative.");

        if (dto.LineNumber <= 0)
            throw new Exception("Line number must be greater than zero.");

        var creditNote = await _context.CreditNotes
            .FirstOrDefaultAsync(x =>
                x.Id == dto.CreditNoteId &&
                x.IsActive);

        if (creditNote == null)
            throw new Exception("Credit note not found.");

        if (creditNote.IsPosted)
            throw new Exception("Posted credit note cannot be modified.");

        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.Id == dto.ProductId &&
                x.IsActive);

        if (product == null)
            throw new Exception("Product not found.");

        var lineExists = await _context.CreditNoteLines
            .AnyAsync(x =>
                x.CreditNoteId == dto.CreditNoteId &&
                x.LineNumber == dto.LineNumber);

        if (lineExists)
            throw new Exception("Line number already exists for this credit note.");

        var taxableAmount =
            (dto.Quantity * dto.Rate) - dto.DiscountAmount;

        if (taxableAmount < 0)
            throw new Exception("Discount cannot be greater than gross amount.");

        var lineTotal = taxableAmount + dto.TaxAmount;

        var entity = new CreditNoteLine
        {
            CreditNoteId = dto.CreditNoteId,
            ProductId = dto.ProductId,
            Description = dto.Description?.Trim(),
            Quantity = dto.Quantity,
            Rate = dto.Rate,
            DiscountAmount = dto.DiscountAmount,
            TaxableAmount = taxableAmount,
            TaxAmount = dto.TaxAmount,
            LineTotal = lineTotal,
            LineNumber = dto.LineNumber
        };

        _context.CreditNoteLines.Add(entity);

        await _context.SaveChangesAsync();

        await UpdateCreditNoteTotalAsync(dto.CreditNoteId);

        dto.Id = entity.Id;
        dto.TaxableAmount = entity.TaxableAmount;
        dto.LineTotal = entity.LineTotal;

        return dto;
    }

    public async Task<bool> UpdateAsync(int id, CreditNoteLineDto dto)
    {
        var entity = await _context.CreditNoteLines
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        var creditNote = await _context.CreditNotes
            .FirstOrDefaultAsync(x =>
                x.Id == entity.CreditNoteId &&
                x.IsActive);

        if (creditNote == null)
            throw new Exception("Credit note not found.");

        if (creditNote.IsPosted)
            throw new Exception("Posted credit note cannot be modified.");

        if (dto.Quantity <= 0)
            throw new Exception("Quantity must be greater than zero.");

        if (dto.Rate < 0)
            throw new Exception("Rate cannot be negative.");

        if (dto.DiscountAmount < 0)
            throw new Exception("Discount amount cannot be negative.");

        if (dto.TaxAmount < 0)
            throw new Exception("Tax amount cannot be negative.");

        var taxableAmount =
            (dto.Quantity * dto.Rate) - dto.DiscountAmount;

        if (taxableAmount < 0)
            throw new Exception("Discount cannot be greater than gross amount.");

        var lineTotal = taxableAmount + dto.TaxAmount;

        entity.ProductId = dto.ProductId;
        entity.Description = dto.Description?.Trim();
        entity.Quantity = dto.Quantity;
        entity.Rate = dto.Rate;
        entity.DiscountAmount = dto.DiscountAmount;
        entity.TaxableAmount = taxableAmount;
        entity.TaxAmount = dto.TaxAmount;
        entity.LineTotal = lineTotal;
        entity.LineNumber = dto.LineNumber;

        await _context.SaveChangesAsync();

        await UpdateCreditNoteTotalAsync(entity.CreditNoteId);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.CreditNoteLines
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        var creditNote = await _context.CreditNotes
            .FirstOrDefaultAsync(x =>
                x.Id == entity.CreditNoteId &&
                x.IsActive);

        if (creditNote == null)
            throw new Exception("Credit note not found.");

        if (creditNote.IsPosted)
            throw new Exception("Posted credit note cannot be modified.");

        _context.CreditNoteLines.Remove(entity);

        await _context.SaveChangesAsync();

        await UpdateCreditNoteTotalAsync(entity.CreditNoteId);

        return true;
    }

    private async Task UpdateCreditNoteTotalAsync(int creditNoteId)
    {
        var total = await _context.CreditNoteLines
            .Where(x => x.CreditNoteId == creditNoteId)
            .SumAsync(x => x.LineTotal);

        var creditNote = await _context.CreditNotes
            .FirstOrDefaultAsync(x => x.Id == creditNoteId);

        if (creditNote != null)
        {
            creditNote.TotalAmount = total;
            creditNote.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}