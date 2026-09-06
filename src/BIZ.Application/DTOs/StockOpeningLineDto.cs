namespace BIZ.Application.DTOs;

public class StockOpeningLineDto
{
    public int Id { get; set; }

    public int StockOpeningId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal TotalCost { get; set; }

    public string? Description { get; set; }

    public int LineNumber { get; set; }
}