namespace BIZ.Application.DTOs;

public class JournalLineDto
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
}