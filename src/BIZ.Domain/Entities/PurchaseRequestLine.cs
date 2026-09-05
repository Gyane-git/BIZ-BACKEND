namespace BIZ.Domain.Entities;

public class PurchaseRequestLine
{
    public int Id { get; set; }

    public int PurchaseRequestId { get; set; }

    public int ProductId { get; set; }

    public int? UnitId { get; set; }

    public string? Description { get; set; }

    public decimal Quantity { get; set; }

    public int LineNumber { get; set; }

    public string? Notes { get; set; }

    public PurchaseRequest PurchaseRequest { get; set; } = null!;
}