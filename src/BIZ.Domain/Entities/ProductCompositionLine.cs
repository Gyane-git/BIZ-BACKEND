namespace BIZ.Domain.Entities;

public class ProductCompositionLine
{
    public int Id { get; set; }

    public int ProductCompositionId { get; set; }

    public int? ComponentProductId { get; set; }

    public string ComponentName { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public string? Unit { get; set; }

    public decimal Percentage { get; set; }

    public string? Description { get; set; }

    public int LineNumber { get; set; }

    public ProductComposition ProductComposition { get; set; } = null!;

    public Product? ComponentProduct { get; set; }
}