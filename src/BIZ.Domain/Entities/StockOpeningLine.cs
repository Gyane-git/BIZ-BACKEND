namespace BIZ.Domain.Entities;

public class StockOpeningLine
{
    public int Id { get; set; }

    public int StockOpeningId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal TotalCost { get; set; }

    public string? Description { get; set; }

    public int LineNumber { get; set; }

    public StockOpening StockOpening { get; set; } = null!;

    public Product Product { get; set; } = null!;
}