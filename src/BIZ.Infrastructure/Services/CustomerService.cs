using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly TenantDbContext _db;

    public CustomerService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<CustomerDto>> GetAllAsync()
    {
        return await _db.Customers
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CustomerDto
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

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        return await _db.Customers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CustomerDto
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

    public async Task<CustomerDto> CreateAsync(CustomerDto dto)
    {
        var code = dto.Code.Trim();

        var exists = await _db.Customers
            .AnyAsync(x => x.Code == code);

        if (exists)
        {
            throw new InvalidOperationException(
                $"Customer code '{code}' already exists.");
        }

        var customer = new Customer
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

        _db.Customers.Add(customer);

        await _db.SaveChangesAsync();

        dto.Id = customer.Id;

        return dto;
    }

    public async Task<bool> UpdateAsync(int id, CustomerDto dto)
    {
        var customer = await _db.Customers
            .FirstOrDefaultAsync(x => x.Id == id);

        if (customer is null)
            return false;

        var code = dto.Code.Trim();

        var duplicate = await _db.Customers
            .AnyAsync(x =>
                x.Code == code &&
                x.Id != id);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Customer code '{code}' already exists.");
        }

        customer.Code = code;
        customer.Name = dto.Name.Trim();
        customer.Address = dto.Address?.Trim();
        customer.Phone = dto.Phone?.Trim();
        customer.Email = dto.Email?.Trim();
        customer.PanNumber = dto.PanNumber?.Trim();
        customer.ContactPerson = dto.ContactPerson?.Trim();
        customer.CreditLimit = dto.CreditLimit;
        customer.CreditDays = dto.CreditDays;
        customer.IsActive = dto.IsActive;
        customer.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await _db.Customers
            .FirstOrDefaultAsync(x => x.Id == id);

        if (customer is null)
            return false;

        _db.Customers.Remove(customer);

        await _db.SaveChangesAsync();

        return true;
    }
}