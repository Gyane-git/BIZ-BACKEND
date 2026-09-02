namespace BIZ.Application.DTOs;

public class CurrencyDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Symbol { get; set; }

    public string? Description { get; set; }

    public bool IsBaseCurrency { get; set; }

    public bool IsActive { get; set; } = true;
}