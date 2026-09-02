namespace BIZ.Application.DTOs;

public class WarehouseLocationDto
{
    public int Id { get; set; }

    public int WarehouseId { get; set; }

    public string? Location { get; set; }

    public string? SubLocation { get; set; }

    public string? Rack { get; set; }

    public string? Col { get; set; }

    public string? ActualLocation { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Memo { get; set; }

    public int? Sequence { get; set; }

    public string? LocCode { get; set; }

    public string? Pcode { get; set; }
}