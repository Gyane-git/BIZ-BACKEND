namespace BIZ.Domain.Entities;

public class ProductComposition
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string CompositionName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    // Navigation
    public Product Product { get; set; } = null!;

    public ICollection<ProductCompositionLine> ProductCompositionLines { get; set; }
        = new List<ProductCompositionLine>();
}