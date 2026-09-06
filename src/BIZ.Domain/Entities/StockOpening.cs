namespace BIZ.Domain.Entities;

public class StockOpening
{
    public int Id { get; set; }

    public int FiscalYearId { get; set; }

    public int FiscalYearPeriodId { get; set; }

    public int? WarehouseId { get; set; }

    public int? BranchId { get; set; }

    public string OpeningNumber { get; set; } = string.Empty;

    public DateTime OpeningDate { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Description { get; set; }

    public decimal TotalQuantity { get; set; }

    public decimal TotalValue { get; set; }

    public string Status { get; set; } = "Draft";

    public bool IsPosted { get; set; }

    public DateTime? PostedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public FiscalYear FiscalYear { get; set; } = null!;

    public FiscalYearPeriod FiscalYearPeriod { get; set; } = null!;

    public ICollection<StockOpeningLine> StockOpeningLines { get; set; }
        = new List<StockOpeningLine>();
}