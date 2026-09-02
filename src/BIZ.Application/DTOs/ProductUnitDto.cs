namespace BIZ.Application.DTOs;

public class ProductUnitDto
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
}