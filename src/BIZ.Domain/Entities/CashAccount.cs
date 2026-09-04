namespace BIZ.Domain.Entities;

public class CashAccount
{
    public int Id { get; set; }

    public int LedgerAccountId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal OpeningBalance { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public LedgerAccount LedgerAccount { get; set; } = null!;
}