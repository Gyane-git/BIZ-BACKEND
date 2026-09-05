namespace BIZ.Domain.Entities;

public class SalesOrderLine
{
    public int Id { get; set; }

    public int SalesOrderId { get; set; }

    public int ProductId { get; set; }
    public int? UnitId { get; set; }

    public string? Description { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }

    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }

    public decimal LineTotal { get; set; }

    public int LineNumber { get; set; }

    public SalesOrder SalesOrder { get; set; } = null!;
}