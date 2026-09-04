namespace BIZ.Domain.Entities;

public class Journal
{
    public int Id { get; set; }

    public int FiscalYearId { get; set; }

    public int FiscalYearPeriodId { get; set; }

    public string JournalNumber { get; set; } = string.Empty;

    public DateTime JournalDate { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Description { get; set; }

    public string JournalType { get; set; } = "General";

    public bool IsPosted { get; set; }

    public DateTime? PostedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public FiscalYear FiscalYear { get; set; } = null!;

    public FiscalYearPeriod FiscalYearPeriod { get; set; } = null!;

    public ICollection<JournalLine> JournalLines { get; set; }
        = new List<JournalLine>();
}