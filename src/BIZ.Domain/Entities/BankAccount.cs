namespace BIZ.Domain.Entities;

public class BankAccount
{
    public int Id { get; set; }

    public int LedgerAccountId { get; set; }

    public string BankName { get; set; } = string.Empty;

    public string? BranchName { get; set; }

    public string AccountName { get; set; } = string.Empty;

    public string AccountNumber { get; set; } = string.Empty;

    public string AccountType { get; set; } = "Current";

    public string CurrencyCode { get; set; } = "NPR";

    public decimal OpeningBalance { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public LedgerAccount LedgerAccount { get; set; } = null!;
}