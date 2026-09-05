namespace BIZ.Application.DTOs;

public class PurchaseRequestDto
{
    public int Id { get; set; }

    public int FiscalYearId { get; set; }

    public int FiscalYearPeriodId { get; set; }

    public string RequestNumber { get; set; } = string.Empty;

    public DateTime RequestDate { get; set; }

    public string? RequiredByDate { get; set; }

    public string Priority { get; set; } = "Normal";

    public string Status { get; set; } = "Draft";

    public string? Purpose { get; set; }

    public string? Notes { get; set; }

    public int? BranchId { get; set; }

    public int? WarehouseId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<PurchaseRequestLineDto> Lines { get; set; } = new();
}