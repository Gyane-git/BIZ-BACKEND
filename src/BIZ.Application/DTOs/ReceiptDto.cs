namespace BIZ.Application.DTOs;

public class ReceiptDto
{
    public int Id { get; set; }

    public int JournalId { get; set; }

    public int LedgerAccountId { get; set; }

    public int? SubLedgerId { get; set; }

    public int? CashAccountId { get; set; }

    public int? BankAccountId { get; set; }

    public string ReceiptNumber { get; set; } = string.Empty;

    public DateTime ReceiptDate { get; set; }

    public decimal Amount { get; set; }

    public string ReceiptMode { get; set; } = "Cash";

    public string? ReferenceNumber { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}