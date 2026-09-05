namespace BIZ.Domain.Entities;

public class SalesPaymentAllocation
{
    public int Id { get; set; }

    public int SalesPaymentId { get; set; }

    public int SalesInvoiceId { get; set; }

    public decimal AllocatedAmount { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public SalesPayment SalesPayment { get; set; } = null!;

    public SalesInvoice SalesInvoice { get; set; } = null!;
}