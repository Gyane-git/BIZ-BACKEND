namespace BIZ.Domain.Entities;

public class StockTransaction
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int? WarehouseId { get; set; }

    public int? BranchId { get; set; }

    public int? FiscalYearId { get; set; }

    public int? FiscalYearPeriodId { get; set; }

    public DateTime TransactionDate { get; set; }

    public string TransactionType { get; set; } = string.Empty;

    public string ReferenceType { get; set; } = string.Empty;

    public int? ReferenceId { get; set; }

    public string? ReferenceNumber { get; set; }

    public decimal QuantityIn { get; set; }

    public decimal QuantityOut { get; set; }

    public decimal BalanceQuantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal TotalCost { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public FiscalYear? FiscalYear { get; set; }
    public FiscalYearPeriod? FiscalYearPeriod { get; set; }

    public Product Product { get; set; } = null!;
}