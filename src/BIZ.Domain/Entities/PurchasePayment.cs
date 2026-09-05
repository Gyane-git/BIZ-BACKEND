namespace BIZ.Domain.Entities;

public class PurchasePayment
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

    public FiscalYear FiscalYear { get; set; } = null!;

    public FiscalYearPeriod FiscalYearPeriod { get; set; } = null!;

    public ICollection<PurchasePaymentAllocation> PurchasePaymentAllocations { get; set; }
        = new List<PurchasePaymentAllocation>();
}