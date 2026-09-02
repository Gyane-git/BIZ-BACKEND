namespace BIZ.Domain.Entities;

public class ProductBarcode
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int? ProductUnitId { get; set; }

    public string Barcode { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Product Product { get; set; } = null!;

    public ProductUnit? ProductUnit { get; set; }
}