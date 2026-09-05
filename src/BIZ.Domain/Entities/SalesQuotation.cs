namespace BIZ.Domain.Entities;

public class SalesQuotation
{
    public int Id { get; set; }

    // Accounting period reference
    public int FiscalYearId { get; set; }
    public int FiscalYearPeriodId { get; set; }

    // Customer
    public int CustomerId { get; set; }

    // Quotation information
    public string QuotationNumber { get; set; } = string.Empty;
    public DateTime QuotationDate { get; set; }
    public DateTime? ValidUntil { get; set; }

    // Commercial information
    public int? CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;

    // Totals
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }

    // Status
    public string Status { get; set; } = "Draft";

    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }

    // Organization
    public int? BranchId { get; set; }
    public int? WarehouseId { get; set; }

    // Audit
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public FiscalYear FiscalYear { get; set; } = null!;
    public FiscalYearPeriod FiscalYearPeriod { get; set; } = null!;

    public ICollection<SalesQuotationLine> SalesQuotationLines { get; set; }
        = new List<SalesQuotationLine>();
}