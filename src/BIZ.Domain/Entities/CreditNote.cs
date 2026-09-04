namespace BIZ.Domain.Entities;

public class CreditNote
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

    public FiscalYear FiscalYear { get; set; } = null!;
    public FiscalYearPeriod FiscalYearPeriod { get; set; } = null!;
    public LedgerAccount LedgerAccount { get; set; } = null!;
    public SubLedger? SubLedger { get; set; }

    public ICollection<CreditNoteLine> CreditNoteLines { get; set; }
        = new List<CreditNoteLine>();
}