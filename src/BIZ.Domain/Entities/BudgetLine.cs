namespace BIZ.Domain.Entities;

public class BudgetLine
{
    public int Id { get; set; }

    public int BudgetId { get; set; }

    public int LedgerAccountId { get; set; }

    public int? CostCenterId { get; set; }

    public decimal BudgetAmount { get; set; }

    public decimal RevisedAmount { get; set; }

    public string? Description { get; set; }

    public int LineNumber { get; set; }

    public Budget Budget { get; set; } = null!;

    public LedgerAccount LedgerAccount { get; set; } = null!;

    public CostCenter? CostCenter { get; set; }
}