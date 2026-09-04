using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IBudgetLineService
{
    Task<List<BudgetLineDto>> GetAllAsync();

    Task<List<BudgetLineDto>> GetByBudgetAsync(int budgetId);

    Task<BudgetLineDto?> GetByIdAsync(int id);

    Task<BudgetLineDto> CreateAsync(BudgetLineDto dto);

    Task<bool> UpdateAsync(int id, BudgetLineDto dto);

    Task<bool> DeleteAsync(int id);
}