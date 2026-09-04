using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IFiscalYearService
{
    Task<List<FiscalYearDto>> GetAllAsync();

    Task<FiscalYearDto?> GetByIdAsync(int id);

    Task<FiscalYearDto?> GetByCodeAsync(string code);

    Task<FiscalYearDto?> GetCurrentAsync();

    Task<FiscalYearDto> CreateAsync(FiscalYearDto dto);

    Task<bool> UpdateAsync(
        int id,
        FiscalYearDto dto);

    Task<bool> CloseAsync(int id);

    Task<bool> DeleteAsync(int id);
}