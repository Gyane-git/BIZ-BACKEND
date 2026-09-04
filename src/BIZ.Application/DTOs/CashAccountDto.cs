namespace BIZ.Application.DTOs;

public class CashAccountDto
{
    public int Id { get; set; }

    public int LedgerAccountId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal OpeningBalance { get; set; }

    public bool IsActive { get; set; } = true;
}