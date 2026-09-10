using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IValueAddedListService
{
    Task<IEnumerable<ValueAddedListDto>> GetAllAsync();

    Task<ValueAddedListDto?> GetByIdAsync(int id);

    Task<ValueAddedListDto> CreateAsync(ValueAddedListDto dto);

    Task<bool> UpdateAsync(int id, ValueAddedListDto dto);

    Task<bool> DeleteAsync(int id);
}