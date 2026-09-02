namespace BIZ.Domain.Entities;

public class Warehouse
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? ShortName { get; set; }

    public string? City { get; set; }

    public string? Address { get; set; }

    public string? TelNo { get; set; }

    public string? MobileNo { get; set; }

    public string? ContactPerson { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<WarehouseLocation> Locations { get; set; }
        = new List<WarehouseLocation>();
}