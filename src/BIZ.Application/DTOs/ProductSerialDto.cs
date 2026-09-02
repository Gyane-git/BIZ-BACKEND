namespace BIZ.Application.DTOs;

public class ProductSerialDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int? ProductVariantId { get; set; }

    public int? ProductBatchId { get; set; }

    public string SerialNumber { get; set; } = string.Empty;

    public DateTime? PurchaseDate { get; set; }

    public DateTime? WarrantyStartDate { get; set; }

    public DateTime? WarrantyEndDate { get; set; }

    public decimal? PurchaseRate { get; set; }

    public decimal? SalesRate { get; set; }

    public string Status { get; set; } = "Available";

    public string? Remarks { get; set; }

    public bool IsActive { get; set; } = true;
}