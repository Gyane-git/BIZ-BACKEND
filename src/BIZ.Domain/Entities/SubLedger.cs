namespace BIZ.Domain.Entities;

public class SubLedger
{
    public int Id { get; set; }

    public int LedgerAccountId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ContactPerson { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? TaxNumber { get; set; }

    public decimal OpeningDebit { get; set; }

    public decimal OpeningCredit { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public LedgerAccount LedgerAccount { get; set; } = null!;
}