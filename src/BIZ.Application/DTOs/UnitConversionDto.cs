namespace BIZ.Application.DTOs;

public class UnitConversionDto
{
    public int Id { get; set; }

    public int FromUnitId { get; set; }

    public int ToUnitId { get; set; }

    public decimal ConversionFactor { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}