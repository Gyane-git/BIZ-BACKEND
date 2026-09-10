namespace BIZ.Application.DTOs;

public class ProductValueAddedDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitCost { get; set; }
    public bool IsPercentage { get; set; }
    public bool IsActive { get; set; } = true;
}
