namespace BIZ.Domain.Entities;

public class ProductAttribute
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string AttributeName { get; set; } = string.Empty;

    public string AttributeValue { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Product Product { get; set; } = null!;
}