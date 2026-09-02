namespace BIZ.Domain.Entities;

public class ProductVariant
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    // Unique variant code/SKU
    public string VariantCode { get; set; } = string.Empty;

    // Variant name
    public string VariantName { get; set; } = string.Empty;

    // Example: Red, Blue, Black
    public string? Color { get; set; }

    // Example: S, M, L, XL
    public string? Size { get; set; }

    // Example: 500ml, 1L, 2kg
    public string? Specification { get; set; }

    // Variant-specific pricing
    public decimal? PurchaseRate { get; set; }

    public decimal? SalesRate { get; set; }

    public decimal? MRP { get; set; }

    public decimal? DealerPrice { get; set; }

    public decimal? DiscountRate { get; set; }

    // Inventory
    public decimal? ReorderLevel { get; set; }

    public decimal? ReorderQty { get; set; }

    // Status / Audit
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
}