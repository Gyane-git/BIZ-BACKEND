namespace BIZ.Domain.Entities;

public class StockBalance
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int? WarehouseId { get; set; }

    public int? BranchId { get; set; }

    public decimal Quantity { get; set; }

    public decimal ReservedQuantity { get; set; }

    public decimal AvailableQuantity { get; set; }

    public decimal AverageCost { get; set; }

    public decimal StockValue { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Product Product { get; set; } = null!;
}