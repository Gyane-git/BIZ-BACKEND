namespace BIZ.Domain.Entities;

public class StockTransferLine
{
    public int Id { get; set; }

    public int StockTransferId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal LineTotal { get; set; }

    public string? Description { get; set; }

    public int LineNumber { get; set; }

    public StockTransfer StockTransfer { get; set; } = null!;

    public Product Product { get; set; } = null!;
}