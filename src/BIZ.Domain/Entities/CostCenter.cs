namespace BIZ.Domain.Entities;

public class CostCenter
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? CompanyUnitId { get; set; }

    public int? BranchId { get; set; }

    public int? DepartmentId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public CompanyUnit? CompanyUnit { get; set; }

    public Branch? Branch { get; set; }

    public Department? Department { get; set; }
}