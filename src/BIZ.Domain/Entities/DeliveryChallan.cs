namespace BIZ.Domain.Entities;

public class DeliveryChallan
{
    public int Id { get; set; }

    public int FiscalYearId { get; set; }
    public int FiscalYearPeriodId { get; set; }

    public int CustomerId { get; set; }

    public int? SalesOrderId { get; set; }

    public string ChallanNumber { get; set; } = string.Empty;

    public DateTime ChallanDate { get; set; }

    public DateTime? ExpectedReturnDate { get; set; }

    public string Status { get; set; } = "Draft";

    public string? VehicleNumber { get; set; }

    public string? DriverName { get; set; }

    public string? DriverContact { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public int? BranchId { get; set; }

    public int? WarehouseId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public FiscalYear FiscalYear { get; set; } = null!;

    public FiscalYearPeriod FiscalYearPeriod { get; set; } = null!;

    public ICollection<DeliveryChallanLine> DeliveryChallanLines { get; set; }
        = new List<DeliveryChallanLine>();
}