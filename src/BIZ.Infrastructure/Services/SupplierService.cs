using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class SupplierService : ISupplierService
{
    private readonly TenantDbContext _db;

    public SupplierService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<SupplierDto>> GetAllAsync()
    {
        return await _db.Suppliers
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SupplierDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Address = x.Address,
                Phone = x.Phone,
                Email = x.Email,
                PanNumber = x.PanNumber,
                ContactPerson = x.ContactPerson,
                CreditLimit = x.CreditLimit,
                CreditDays = x.CreditDays,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        return await _db.Suppliers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new SupplierDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Address = x.Address,
                Phone = x.Phone,
                Email = x.Email,
                PanNumber = x.PanNumber,
                ContactPerson = x.ContactPerson,
                CreditLimit = x.CreditLimit,
                CreditDays = x.CreditDays,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SupplierDto> CreateAsync(SupplierDto dto)
    {
        var code = dto.Code.Trim();

        if (await _db.Suppliers.AnyAsync(x => x.Code == code))
        {
            throw new InvalidOperationException(
                $"Supplier code '{code}' already exists.");
        }

        var supplier = new Supplier
        {
            Code = code,
            Name = dto.Name.Trim(),
            Address = dto.Address?.Trim(),
            Phone = dto.Phone?.Trim(),
            Email = dto.Email?.Trim(),
            PanNumber = dto.PanNumber?.Trim(),
            ContactPerson = dto.ContactPerson?.Trim(),
            CreditLimit = dto.CreditLimit,
            CreditDays = dto.CreditDays,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Suppliers.Add(supplier);

        await _db.SaveChangesAsync();

        dto.Id = supplier.Id;

        return dto;
    }

    public async Task<bool> UpdateAsync(int id, SupplierDto dto)
    {
        var supplier = await _db.Suppliers
            .FirstOrDefaultAsync(x => x.Id == id);

        if (supplier is null)
            return false;

        var code = dto.Code.Trim();

        if (await _db.Suppliers.AnyAsync(
            x => x.Code == code && x.Id != id))
        {
            throw new InvalidOperationException(
                $"Supplier code '{code}' already exists.");
        }

        supplier.Code = code;
        supplier.Name = dto.Name.Trim();
        supplier.Address = dto.Address?.Trim();
        supplier.Phone = dto.Phone?.Trim();
        supplier.Email = dto.Email?.Trim();
        supplier.PanNumber = dto.PanNumber?.Trim();
        supplier.ContactPerson = dto.ContactPerson?.Trim();
        supplier.CreditLimit = dto.CreditLimit;
        supplier.CreditDays = dto.CreditDays;
        supplier.IsActive = dto.IsActive;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var supplier = await _db.Suppliers
            .FirstOrDefaultAsync(x => x.Id == id);

        if (supplier is null)
            return false;

        _db.Suppliers.Remove(supplier);

        await _db.SaveChangesAsync();

        return true;
    }
}