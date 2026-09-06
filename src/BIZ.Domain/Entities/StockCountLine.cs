namespace BIZ.Domain.Entities;

public class StockCountLine
{
    public int Id { get; set; }

    public int StockCountId { get; set; }

    public int ProductId { get; set; }

    public decimal SystemQuantity { get; set; }

    public decimal CountedQuantity { get; set; }

    public decimal DifferenceQuantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal DifferenceValue { get; set; }

    public string? Description { get; set; }

    public int LineNumber { get; set; }

    public StockCount StockCount { get; set; } = null!;

    public Product Product { get; set; } = null!;
}