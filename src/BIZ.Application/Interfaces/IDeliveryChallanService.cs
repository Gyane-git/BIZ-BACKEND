using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IDeliveryChallanService
{
    Task<IEnumerable<DeliveryChallanDto>> GetAllAsync();

    Task<DeliveryChallanDto?> GetByIdAsync(int id);

    Task<DeliveryChallanDto> CreateAsync(
        DeliveryChallanDto dto);

    Task<bool> UpdateAsync(
        int id,
        DeliveryChallanDto dto);

    Task<bool> DeleteAsync(int id);
}