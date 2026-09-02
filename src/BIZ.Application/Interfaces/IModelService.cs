using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IModelService
{
    Task<List<ModelDto>> GetAllAsync();

    Task<ModelDto?> GetByIdAsync(int id);

    Task<ModelDto> CreateAsync(ModelDto dto);

    Task<bool> UpdateAsync(int id, ModelDto dto);

    Task<bool> DeleteAsync(int id);
}