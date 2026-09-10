namespace BIZ.Application.DTOs;

public class ValueAddedListDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal DefaultAmount { get; set; }

    public string AmountType { get; set; } = "Fixed";

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}