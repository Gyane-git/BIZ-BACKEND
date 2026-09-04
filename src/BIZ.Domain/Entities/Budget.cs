namespace BIZ.Domain.Entities;

public class Budget
{
    public int Id { get; set; }

    public int FiscalYearId { get; set; }

    public int? CostCenterId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal TotalAmount { get; set; }

    public bool IsApproved { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public FiscalYear FiscalYear { get; set; } = null!;

    public CostCenter? CostCenter { get; set; }

    public ICollection<BudgetLine> BudgetLines { get; set; }
        = new List<BudgetLine>();
}