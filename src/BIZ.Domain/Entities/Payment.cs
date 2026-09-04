namespace BIZ.Domain.Entities;

public class Payment
{
    public int Id { get; set; }

    public int JournalId { get; set; }

    public int LedgerAccountId { get; set; }

    public int? SubLedgerId { get; set; }

    public int? CashAccountId { get; set; }

    public int? BankAccountId { get; set; }

    public string PaymentNumber { get; set; } = string.Empty;

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMode { get; set; } = "Cash";

    public string? ReferenceNumber { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Journal Journal { get; set; } = null!;

    public LedgerAccount LedgerAccount { get; set; } = null!;

    public SubLedger? SubLedger { get; set; }

    public CashAccount? CashAccount { get; set; }

    public BankAccount? BankAccount { get; set; }
}