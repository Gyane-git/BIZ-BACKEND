namespace BIZ.Application.DTOs;

public class GoodsReceiptDto
{
    public int Id { get; set; }

    public int FiscalYearId { get; set; }
    public int FiscalYearPeriodId { get; set; }

    public int SupplierId { get; set; }
    public int PurchaseOrderId { get; set; }

    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }

    public int? WarehouseId { get; set; }

    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }

    public string Status { get; set; } = "Draft";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<GoodsReceiptLineDto> Lines { get; set; }
        = new();
}