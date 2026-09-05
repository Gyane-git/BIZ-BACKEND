namespace BIZ.Domain.Entities;

public class PurchasePaymentAllocation
{
    public int Id { get; set; }

    public int PurchasePaymentId { get; set; }

    public int PurchaseInvoiceId { get; set; }

    public decimal AllocatedAmount { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public PurchasePayment PurchasePayment { get; set; } = null!;

    public PurchaseInvoice PurchaseInvoice { get; set; } = null!;
}