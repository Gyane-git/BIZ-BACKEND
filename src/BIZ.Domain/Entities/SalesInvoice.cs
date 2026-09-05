namespace BIZ.Domain.Entities;

public class SalesInvoice
{
    public int Id { get; set; }

    public int FiscalYearId { get; set; }
    public int FiscalYearPeriodId { get; set; }

    public int CustomerId { get; set; }

    public int? SalesOrderId { get; set; }
    public int? DeliveryChallanId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }

    public int? CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }

    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }

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

    public ICollection<SalesInvoiceLine> SalesInvoiceLines { get; set; }
        = new List<SalesInvoiceLine>();
}