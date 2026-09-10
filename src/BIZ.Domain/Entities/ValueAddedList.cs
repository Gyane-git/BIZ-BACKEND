namespace BIZ.Domain.Entities;

public class ValueAddedList
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal DefaultAmount { get; set; }

    public string AmountType { get; set; } = "Fixed";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}