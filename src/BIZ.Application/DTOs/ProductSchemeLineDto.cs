namespace BIZ.Application.DTOs;

public class ProductSchemeLineDto
{
    public int Id { get; set; }

    public int ProductSchemeId { get; set; }

    public int ProductId { get; set; }

    public decimal MinimumQuantity { get; set; }

    public decimal? MaximumQuantity { get; set; }

    public decimal DiscountPercent { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal FreeQuantity { get; set; }

    public string? Description { get; set; }

    public int LineNumber { get; set; }
}