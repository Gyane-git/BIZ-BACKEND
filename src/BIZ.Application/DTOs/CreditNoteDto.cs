namespace BIZ.Application.DTOs;

public class CreditNoteDto
{
    public int Id { get; set; }

    public int FiscalYearId { get; set; }
    public int FiscalYearPeriodId { get; set; }

    public int LedgerAccountId { get; set; }
    public int? SubLedgerId { get; set; }

    public string CreditNoteNumber { get; set; } = string.Empty;
    public DateTime CreditNoteDate { get; set; }

    public string? ReferenceNumber { get; set; }
    public string? Reason { get; set; }

    public decimal TotalAmount { get; set; }

    public bool IsPosted { get; set; }
    public DateTime? PostedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}