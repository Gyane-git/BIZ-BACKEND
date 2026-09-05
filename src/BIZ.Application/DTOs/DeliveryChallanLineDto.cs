namespace BIZ.Application.DTOs;

public class DeliveryChallanLineDto
{
    public int Id { get; set; }

    public int DeliveryChallanId { get; set; }

    public int ProductId { get; set; }

    public int? UnitId { get; set; }

    public string? Description { get; set; }

    public decimal Quantity { get; set; }

    public int LineNumber { get; set; }
}