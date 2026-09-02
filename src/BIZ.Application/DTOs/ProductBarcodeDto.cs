namespace BIZ.Application.DTOs;

public class ProductBarcodeDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int? ProductUnitId { get; set; }

    public string Barcode { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; } = true;
}