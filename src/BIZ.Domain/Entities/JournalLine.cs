namespace BIZ.Domain.Entities;

public class JournalLine
{
    public int Id { get; set; }

    public int JournalId { get; set; }

    public int LedgerAccountId { get; set; }

    public int? SubLedgerId { get; set; }

    public int? CostCenterId { get; set; }

    public string? Description { get; set; }

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    public int LineNumber { get; set; }

    public Journal Journal { get; set; } = null!;

    public LedgerAccount LedgerAccount { get; set; } = null!;

    public SubLedger? SubLedger { get; set; }

    public CostCenter? CostCenter { get; set; }
}