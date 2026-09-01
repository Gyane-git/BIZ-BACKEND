namespace BIZ.Domain.Entities;

public class UnitConversion
{
    public int Id { get; set; }

    public int FromUnitId { get; set; }

    public int ToUnitId { get; set; }

    public decimal ConversionFactor { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Unit FromUnit { get; set; } = null!;

    public Unit ToUnit { get; set; } = null!;
}