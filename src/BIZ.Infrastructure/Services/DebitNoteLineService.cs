using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class DebitNoteLineService : IDebitNoteLineService
{
    private readonly TenantDbContext _context;

    public DebitNoteLineService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<List<DebitNoteLineDto>> GetAllAsync()
    {
        return await _context.DebitNoteLines
            .AsNoTracking()
            .OrderBy(x => x.DebitNoteId)
            .ThenBy(x => x.LineNumber)
            .Select(x => new DebitNoteLineDto
            {
                Id = x.Id,
                DebitNoteId = x.DebitNoteId,
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

    public async Task<List<DebitNoteLineDto>> GetByDebitNoteAsync(
        int debitNoteId)
    {
        return await _context.DebitNoteLines
            .AsNoTracking()
            .Where(x => x.DebitNoteId == debitNoteId)
            .OrderBy(x => x.LineNumber)
            .Select(x => new DebitNoteLineDto
            {
                Id = x.Id,
                DebitNoteId = x.DebitNoteId,
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

    public async Task<DebitNoteLineDto?> GetByIdAsync(int id)
    {
        return await _context.DebitNoteLines
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new DebitNoteLineDto
            {
                Id = x.Id,
                DebitNoteId = x.DebitNoteId,
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

    public async Task<DebitNoteLineDto> CreateAsync(
        DebitNoteLineDto dto)
    {
        if (dto.Quantity <= 0)
            throw new Exception(
                "Quantity must be greater than zero.");

        if (dto.Rate < 0)
            throw new Exception(
                "Rate cannot be negative.");

        if (dto.DiscountAmount < 0)
            throw new Exception(
                "Discount amount cannot be negative.");

        if (dto.TaxAmount < 0)
            throw new Exception(
                "Tax amount cannot be negative.");

        if (dto.LineNumber <= 0)
            throw new Exception(
                "Line number must be greater than zero.");

        var debitNote = await _context.DebitNotes
            .FirstOrDefaultAsync(x =>
                x.Id == dto.DebitNoteId &&
                x.IsActive);

        if (debitNote == null)
            throw new Exception("Debit note not found.");

        if (debitNote.IsPosted)
            throw new Exception(
                "Posted debit note cannot be modified.");

        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.Id == dto.ProductId &&
                x.IsActive);

        if (product == null)
            throw new Exception("Product not found.");

        var lineExists = await _context.DebitNoteLines
            .AnyAsync(x =>
                x.DebitNoteId == dto.DebitNoteId &&
                x.LineNumber == dto.LineNumber);

        if (lineExists)
            throw new Exception(
                "Line number already exists for this debit note.");

        var grossAmount = dto.Quantity * dto.Rate;

        var taxableAmount =
            grossAmount - dto.DiscountAmount;

        if (taxableAmount < 0)
            throw new Exception(
                "Discount cannot be greater than gross amount.");

        var lineTotal =
            taxableAmount + dto.TaxAmount;

        var entity = new DebitNoteLine
        {
            DebitNoteId = dto.DebitNoteId,
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

        _context.DebitNoteLines.Add(entity);

        await _context.SaveChangesAsync();

        await UpdateDebitNoteTotalAsync(dto.DebitNoteId);

        dto.Id = entity.Id;
        dto.TaxableAmount = entity.TaxableAmount;
        dto.LineTotal = entity.LineTotal;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        DebitNoteLineDto dto)
    {
        var entity = await _context.DebitNoteLines
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        var debitNote = await _context.DebitNotes
            .FirstOrDefaultAsync(x =>
                x.Id == entity.DebitNoteId &&
                x.IsActive);

        if (debitNote == null)
            throw new Exception("Debit note not found.");

        if (debitNote.IsPosted)
            throw new Exception(
                "Posted debit note cannot be modified.");

        if (dto.Quantity <= 0)
            throw new Exception(
                "Quantity must be greater than zero.");

        if (dto.Rate < 0)
            throw new Exception(
                "Rate cannot be negative.");

        if (dto.DiscountAmount < 0)
            throw new Exception(
                "Discount amount cannot be negative.");

        if (dto.TaxAmount < 0)
            throw new Exception(
                "Tax amount cannot be negative.");

        var lineExists = await _context.DebitNoteLines
            .AnyAsync(x =>
                x.Id != id &&
                x.DebitNoteId == entity.DebitNoteId &&
                x.LineNumber == dto.LineNumber);

        if (lineExists)
            throw new Exception(
                "Line number already exists for this debit note.");

        var grossAmount =
            dto.Quantity * dto.Rate;

        var taxableAmount =
            grossAmount - dto.DiscountAmount;

        if (taxableAmount < 0)
            throw new Exception(
                "Discount cannot be greater than gross amount.");

        var lineTotal =
            taxableAmount + dto.TaxAmount;

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

        await UpdateDebitNoteTotalAsync(entity.DebitNoteId);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.DebitNoteLines
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        var debitNote = await _context.DebitNotes
            .FirstOrDefaultAsync(x =>
                x.Id == entity.DebitNoteId &&
                x.IsActive);

        if (debitNote == null)
            throw new Exception("Debit note not found.");

        if (debitNote.IsPosted)
            throw new Exception(
                "Posted debit note cannot be modified.");

        var debitNoteId = entity.DebitNoteId;

        _context.DebitNoteLines.Remove(entity);

        await _context.SaveChangesAsync();

        await UpdateDebitNoteTotalAsync(debitNoteId);

        return true;
    }

    private async Task UpdateDebitNoteTotalAsync(
        int debitNoteId)
    {
        var total = await _context.DebitNoteLines
            .Where(x => x.DebitNoteId == debitNoteId)
            .SumAsync(x => x.LineTotal);

        var debitNote = await _context.DebitNotes
            .FirstOrDefaultAsync(x => x.Id == debitNoteId);

        if (debitNote != null)
        {
            debitNote.TotalAmount = total;
            debitNote.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}