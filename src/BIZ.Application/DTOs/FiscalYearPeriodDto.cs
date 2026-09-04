namespace BIZ.Application.DTOs;

public class FiscalYearPeriodDto
{
    public int Id { get; set; }

    public int FiscalYearId { get; set; }

    public int PeriodNumber { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsCurrent { get; set; }

    public bool IsClosed { get; set; }

    public bool IsActive { get; set; } = true;
}