namespace BIZ.Domain.Entities;

public class StockTransfer
{
    public int Id { get; set; }

    public int FiscalYearId { get; set; }
    public int FiscalYearPeriodId { get; set; }

    public int? FromWarehouseId { get; set; }
    public int? ToWarehouseId { get; set; }

    public int? FromBranchId { get; set; }
    public int? ToBranchId { get; set; }

    public string TransferNumber { get; set; } = string.Empty;

    public DateTime TransferDate { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Reason { get; set; }

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

    public ICollection<StockTransferLine> StockTransferLines { get; set; }
        = new List<StockTransferLine>();
}