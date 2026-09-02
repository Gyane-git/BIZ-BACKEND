namespace BIZ.Domain.Entities;

public class Product
{
    public int Id { get; set; }

    // Basic Information
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;

    // Classification
    public string? Category { get; set; }
    public string? ValuationMethod { get; set; }

    public string? ProductGroupCode { get; set; }
    public string? ProductSubGroupCode { get; set; }

    // Pricing
    public decimal? MRP { get; set; }
    public decimal? TradeRate { get; set; }
    public decimal? BuyRate { get; set; }
    public decimal? SalesRate { get; set; }
    public decimal? DealerPrice { get; set; }
    public decimal? DiscountRate { get; set; }
    public decimal? Margin { get; set; }

    // Tax
    public decimal? Vat { get; set; }
    public decimal? ExciseRate { get; set; }
    public decimal? BeforeVat { get; set; }

    // Inventory
    public decimal? MaxStock { get; set; }
    public decimal? ReorderLevel { get; set; }
    public decimal? ReorderQty { get; set; }

    public string? CurrencyCode { get; set; }

    // Product Tracking
    public bool HasBatch { get; set; }
    public bool HasExpiryDate { get; set; }
    public bool HasManufacturingDate { get; set; }

    // Other
    public bool IsFavourite { get; set; }
    public bool IsInsurableItem { get; set; }
    public bool IsRestaurantProduct { get; set; }

    public int? ProductPoint { get; set; }

    public string? HSCode { get; set; }

    // Accounting references
    public string? PurchaseGLCode { get; set; }
    public string? PurchaseReturnGLCode { get; set; }
    public string? SalesGLCode { get; set; }
    public string? SalesReturnGLCode { get; set; }

    // Status / Audit
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<ProductUnit> ProductUnits { get; set; }
        = new List<ProductUnit>();

    public ICollection<ProductBarcode> ProductBarcodes { get; set; }
    = new List<ProductBarcode>();
    
    public ICollection<ProductImage> ProductImages { get; set; }
    = new List<ProductImage>();
}