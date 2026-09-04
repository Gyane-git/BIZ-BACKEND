namespace BIZ.Application.DTOs;

public class JournalDto
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
}