namespace BIZ.Application.DTOs;

public class SalesPaymentAllocationDto
{
    public int Id { get; set; }

    public int SalesPaymentId { get; set; }

    public int SalesInvoiceId { get; set; }

    public decimal AllocatedAmount { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}