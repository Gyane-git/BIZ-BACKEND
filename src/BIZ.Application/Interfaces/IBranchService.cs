using BIZ.Domain.Entities;

namespace BIZ.Application.Interfaces;

public interface IBranchService
{
    Task<List<Branch>> GetAllAsync();

    Task<Branch?> GetByIdAsync(int id);

    Task<Branch> CreateAsync(Branch branch);

    Task<bool> UpdateAsync(int id, Branch branch);

    Task<bool> DeleteAsync(int id);
}