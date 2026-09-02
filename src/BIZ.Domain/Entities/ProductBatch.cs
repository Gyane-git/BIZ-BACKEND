namespace BIZ.Domain.Entities;

public class ProductBatch
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int? ProductVariantId { get; set; }

    public string BatchNumber { get; set; } = string.Empty;

    public DateTime? ManufacturingDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public decimal OpeningQuantity { get; set; }

    public decimal CurrentQuantity { get; set; }

    public decimal? PurchaseRate { get; set; }

    public decimal? SalesRate { get; set; }

    public decimal? MRP { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Product Product { get; set; } = null!;

    public ProductVariant? ProductVariant { get; set; }
}