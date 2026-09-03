using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ICostCenterService
{
    Task<List<CostCenterDto>> GetAllAsync();

    Task<CostCenterDto?> GetByIdAsync(int id);

    Task<CostCenterDto?> GetByCodeAsync(string code);

    Task<List<CostCenterDto>> GetByBranchAsync(int branchId);

    Task<List<CostCenterDto>> GetByDepartmentAsync(int departmentId);

    Task<CostCenterDto> CreateAsync(CostCenterDto dto);

    Task<bool> UpdateAsync(
        int id,
        CostCenterDto dto);

    Task<bool> DeleteAsync(int id);
}