namespace BIZ.Domain.Entities;

public class PurchaseOrder
{
    public int Id { get; set; }

    public int FiscalYearId { get; set; }

    public int FiscalYearPeriodId { get; set; }

    public int SupplierId { get; set; }

    public int? PurchaseRequestId { get; set; }

    public int? PurchaseQuotationId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }

    public int? CurrencyId { get; set; }

    public decimal ExchangeRate { get; set; } = 1m;

    public decimal SubTotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal GrandTotal { get; set; }

    public string Status { get; set; } = "Draft";

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public int? BranchId { get; set; }

    public int? WarehouseId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public FiscalYear FiscalYear { get; set; } = null!;

    public FiscalYearPeriod FiscalYearPeriod { get; set; } = null!;

    public ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; }
        = new List<PurchaseOrderLine>();
}