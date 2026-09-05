using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IDeliveryChallanLineService
{
    Task<IEnumerable<DeliveryChallanLineDto>> GetAllAsync();

    Task<DeliveryChallanLineDto?> GetByIdAsync(int id);

    Task<DeliveryChallanLineDto> CreateAsync(
        DeliveryChallanLineDto dto);

    Task<bool> UpdateAsync(
        int id,
        DeliveryChallanLineDto dto);

    Task<bool> DeleteAsync(int id);
}