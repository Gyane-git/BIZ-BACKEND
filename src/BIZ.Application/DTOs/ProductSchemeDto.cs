namespace BIZ.Application.DTOs;

public class ProductSchemeDto
{
    public int Id { get; set; }

    public string SchemeCode { get; set; } = string.Empty;

    public string SchemeName { get; set; } = string.Empty;

    public string SchemeType { get; set; } = "Quantity";

    public DateTime FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<ProductSchemeLineDto> Lines { get; set; } = new();
}