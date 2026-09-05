namespace BIZ.Domain.Entities;

public class SalesQuotationLine
{
    public int Id { get; set; }

    public int SalesQuotationId { get; set; }

    // Product
    public int ProductId { get; set; }

    // Optional unit
    public int? UnitId { get; set; }

    public string? Description { get; set; }

    // Quantity & pricing
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    // Discount
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }

    // Tax
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }

    // Final line amount
    public decimal LineTotal { get; set; }

    public int LineNumber { get; set; }

    // Navigation
    public SalesQuotation SalesQuotation { get; set; } = null!;
}