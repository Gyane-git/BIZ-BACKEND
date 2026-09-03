namespace BIZ.Domain.Entities;

public class LedgerAccount
{
    public int Id { get; set; }

    // Account hierarchy
    public int AccountSubGroupId { get; set; }

    // Basic information
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Account settings
    public string AccountType { get; set; } = "General";

    public bool IsControlAccount { get; set; }

    public bool AllowManualEntry { get; set; } = true;

    public bool IsReconciliationRequired { get; set; }

    // Opening balance
    public decimal OpeningDebit { get; set; }

    public decimal OpeningCredit { get; set; }

    // Status
    public bool IsActive { get; set; } = true;

    // Audit
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public AccountSubGroup AccountSubGroup { get; set; } = null!;
    public ICollection<SubLedger> SubLedgers { get; set; } = new List<SubLedger>();
}