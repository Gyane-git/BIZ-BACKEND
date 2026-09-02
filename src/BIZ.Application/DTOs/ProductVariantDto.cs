namespace BIZ.Application.DTOs;

public class ProductVariantDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string VariantCode { get; set; } = string.Empty;

    public string VariantName { get; set; } = string.Empty;

    public string? Color { get; set; }

    public string? Size { get; set; }

    public string? Specification { get; set; }

    public decimal? PurchaseRate { get; set; }

    public decimal? SalesRate { get; set; }

    public decimal? MRP { get; set; }

    public decimal? DealerPrice { get; set; }

    public decimal? DiscountRate { get; set; }

    public decimal? ReorderLevel { get; set; }

    public decimal? ReorderQty { get; set; }

    public bool IsActive { get; set; } = true;
}