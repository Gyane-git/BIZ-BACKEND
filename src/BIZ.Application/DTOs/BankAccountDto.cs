namespace BIZ.Application.DTOs;

public class BankAccountDto
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
}