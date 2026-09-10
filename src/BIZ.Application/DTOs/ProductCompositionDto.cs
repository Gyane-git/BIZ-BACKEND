namespace BIZ.Application.DTOs;

public class ProductCompositionDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string CompositionName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<ProductCompositionLineDto> Lines { get; set; } = new();
}