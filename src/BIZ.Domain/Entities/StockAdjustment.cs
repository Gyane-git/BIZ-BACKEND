namespace BIZ.Domain.Entities;

public class StockAdjustment
{
    public int Id { get; set; }

    public int FiscalYearId { get; set; }
    public int FiscalYearPeriodId { get; set; }

    public int? WarehouseId { get; set; }
    public int? BranchId { get; set; }

    public string AdjustmentNumber { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }

    public string AdjustmentType { get; set; } = "Increase";

    public string? ReferenceNumber { get; set; }
    public string? Reason { get; set; }

    public decimal TotalIncreaseQuantity { get; set; }
    public decimal TotalDecreaseQuantity { get; set; }
    public decimal TotalValue { get; set; }

    public string Status { get; set; } = "Draft";

    public bool IsPosted { get; set; }
    public DateTime? PostedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public FiscalYear FiscalYear { get; set; } = null!;
    public FiscalYearPeriod FiscalYearPeriod { get; set; } = null!;

    public ICollection<StockAdjustmentLine> StockAdjustmentLines { get; set; }
        = new List<StockAdjustmentLine>();
}