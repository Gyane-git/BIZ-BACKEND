namespace BIZ.Application.DTOs;

public class PurchasePaymentAllocationDto
{
    public int Id { get; set; }

    public int PurchasePaymentId { get; set; }

    public int PurchaseInvoiceId { get; set; }

    public decimal AllocatedAmount { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}