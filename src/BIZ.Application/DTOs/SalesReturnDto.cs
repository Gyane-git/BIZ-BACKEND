namespace BIZ.Application.DTOs;

public class SalesReturnDto
{
    public int Id { get; set; }

    public int FiscalYearId { get; set; }

    public int FiscalYearPeriodId { get; set; }

    public int CustomerId { get; set; }

    public int? SalesInvoiceId { get; set; }

    public int? DeliveryChallanId { get; set; }

    public string ReturnNumber { get; set; } = string.Empty;

    public DateTime ReturnDate { get; set; }

    public decimal SubTotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal GrandTotal { get; set; }

    public string Status { get; set; } = "Draft";

    public string? Reason { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public int? BranchId { get; set; }

    public int? WarehouseId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<SalesReturnLineDto> Lines { get; set; } = new();
}