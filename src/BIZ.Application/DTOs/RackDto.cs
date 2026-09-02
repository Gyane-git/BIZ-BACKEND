namespace BIZ.Application.DTOs;

public class RackDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int? WarehouseId { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}