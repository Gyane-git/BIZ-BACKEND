namespace BIZ.Application.DTOs;

public class ProductAttributeDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string AttributeName { get; set; } = string.Empty;

    public string AttributeValue { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}