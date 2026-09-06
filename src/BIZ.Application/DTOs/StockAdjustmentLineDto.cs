namespace BIZ.Application.DTOs;

public class StockAdjustmentLineDto
{
    public int Id { get; set; }

    public int StockAdjustmentId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal LineTotal { get; set; }

    public string AdjustmentType { get; set; } = "Increase";

    public string? Description { get; set; }

    public int LineNumber { get; set; }
}