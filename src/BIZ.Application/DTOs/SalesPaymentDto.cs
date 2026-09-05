namespace BIZ.Application.DTOs;

public class SalesPaymentDto
{
    public int Id { get; set; }

    public int FiscalYearId { get; set; }

    public int FiscalYearPeriodId { get; set; }

    public int CustomerId { get; set; }

    public string PaymentNumber { get; set; } = string.Empty;

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMode { get; set; } = "Cash";

    public string? ReferenceNumber { get; set; }

    public string? Description { get; set; }

    public int? CashAccountId { get; set; }

    public int? BankAccountId { get; set; }

    public int? JournalId { get; set; }

    public string Status { get; set; } = "Draft";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<SalesPaymentAllocationDto> Allocations { get; set; } = new();
}