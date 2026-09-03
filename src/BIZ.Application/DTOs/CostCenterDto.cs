namespace BIZ.Application.DTOs;

public class CostCenterDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? CompanyUnitId { get; set; }

    public int? BranchId { get; set; }

    public int? DepartmentId { get; set; }

    public bool IsActive { get; set; } = true;
}