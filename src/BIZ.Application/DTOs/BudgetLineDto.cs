namespace BIZ.Application.DTOs;

public class BudgetLineDto
{
    public int Id { get; set; }

    public int BudgetId { get; set; }

    public int LedgerAccountId { get; set; }

    public int? CostCenterId { get; set; }

    public decimal BudgetAmount { get; set; }

    public decimal RevisedAmount { get; set; }

    public string? Description { get; set; }

    public int LineNumber { get; set; }
}