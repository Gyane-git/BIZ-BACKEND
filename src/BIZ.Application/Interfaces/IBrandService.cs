using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IBrandService
{
    Task<List<BrandDto>> GetAllAsync();

    Task<BrandDto?> GetByIdAsync(int id);

    Task<BrandDto> CreateAsync(BrandDto dto);

    Task<bool> UpdateAsync(int id, BrandDto dto);

    Task<bool> DeleteAsync(int id);
}