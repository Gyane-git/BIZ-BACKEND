namespace BIZ.Domain.Entities;

public class StockCount
{
    public int Id { get; set; }

    public int FiscalYearId { get; set; }

    public int FiscalYearPeriodId { get; set; }

    public int? WarehouseId { get; set; }

    public int? BranchId { get; set; }

    public string CountNumber { get; set; } = string.Empty;

    public DateTime CountDate { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Reason { get; set; }

    public decimal TotalSystemQuantity { get; set; }

    public decimal TotalCountedQuantity { get; set; }

    public decimal TotalDifferenceQuantity { get; set; }

    public decimal TotalDifferenceValue { get; set; }

    public string Status { get; set; } = "Draft";

    public bool IsPosted { get; set; }

    public DateTime? PostedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public FiscalYear FiscalYear { get; set; } = null!;

    public FiscalYearPeriod FiscalYearPeriod { get; set; } = null!;

    public ICollection<StockCountLine> StockCountLines { get; set; }
        = new List<StockCountLine>();
}