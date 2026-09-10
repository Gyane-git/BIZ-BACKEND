namespace BIZ.Domain.Entities;

public class ProductScheme
{
    public int Id { get; set; }

    public string SchemeCode { get; set; } = string.Empty;

    public string SchemeName { get; set; } = string.Empty;

    public string SchemeType { get; set; } = "Quantity";

    public DateTime FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<ProductSchemeLine> ProductSchemeLines { get; set; }
        = new List<ProductSchemeLine>();
}