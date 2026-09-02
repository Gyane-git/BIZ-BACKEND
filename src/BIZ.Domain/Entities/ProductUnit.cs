namespace BIZ.Domain.Entities;

public class ProductUnit
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int UnitId { get; set; }

    public decimal ConversionQuantity { get; set; } = 1;

    public bool IsBaseUnit { get; set; }

    public bool IsPurchaseUnit { get; set; }

    public bool IsSalesUnit { get; set; }

    public decimal? PurchaseRate { get; set; }

    public decimal? SalesRate { get; set; }

    public decimal? MRP { get; set; }

    public bool IsActive { get; set; } = true;

    public Product Product { get; set; } = null!;

    public Unit Unit { get; set; } = null!;
}