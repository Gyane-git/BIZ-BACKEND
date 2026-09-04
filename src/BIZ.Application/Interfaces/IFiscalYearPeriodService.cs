using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IFiscalYearPeriodService
{
    Task<List<FiscalYearPeriodDto>> GetAllAsync();

    Task<List<FiscalYearPeriodDto>> GetByFiscalYearAsync(
        int fiscalYearId);

    Task<FiscalYearPeriodDto?> GetByIdAsync(int id);

    Task<FiscalYearPeriodDto?> GetByCodeAsync(string code);

    Task<FiscalYearPeriodDto?> GetCurrentAsync(
        int fiscalYearId);

    Task<FiscalYearPeriodDto> CreateAsync(
        FiscalYearPeriodDto dto);

    Task<bool> UpdateAsync(
        int id,
        FiscalYearPeriodDto dto);

    Task<bool> CloseAsync(int id);

    Task<bool> DeleteAsync(int id);
}