using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IBudgetService
{
    Task<List<BudgetDto>> GetAllAsync();

    Task<BudgetDto?> GetByIdAsync(int id);

    Task<BudgetDto?> GetByCodeAsync(string code);

    Task<List<BudgetDto>> GetByFiscalYearAsync(int fiscalYearId);

    Task<BudgetDto> CreateAsync(BudgetDto dto);

    Task<bool> UpdateAsync(int id, BudgetDto dto);

    Task<bool> ApproveAsync(int id);

    Task<bool> DeleteAsync(int id);
}