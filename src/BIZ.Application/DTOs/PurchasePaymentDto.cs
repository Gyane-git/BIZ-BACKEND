namespace BIZ.Application.DTOs;

public class PurchasePaymentDto
{
    public int Id { get; set; }

    public int FiscalYearId { get; set; }

    public int FiscalYearPeriodId { get; set; }

    public int SupplierId { get; set; }

    public int JournalId { get; set; }

    public int? CashAccountId { get; set; }

    public int? BankAccountId { get; set; }

    public string PaymentNumber { get; set; } = string.Empty;

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMode { get; set; } = "Cash";

    public string? ReferenceNumber { get; set; }

    public string? Description { get; set; }

    public bool IsPosted { get; set; }

    public DateTime? PostedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<PurchasePaymentAllocationDto> Allocations { get; set; }
        = new();
}