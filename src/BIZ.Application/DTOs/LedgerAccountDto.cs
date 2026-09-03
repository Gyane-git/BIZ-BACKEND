namespace BIZ.Application.DTOs;

public class LedgerAccountDto
{
    public int Id { get; set; }

    public int AccountSubGroupId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string AccountType { get; set; } = "General";

    public bool IsControlAccount { get; set; }

    public bool AllowManualEntry { get; set; } = true;

    public bool IsReconciliationRequired { get; set; }

    public decimal OpeningDebit { get; set; }

    public decimal OpeningCredit { get; set; }

    public bool IsActive { get; set; } = true;
}