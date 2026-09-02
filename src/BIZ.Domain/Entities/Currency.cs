namespace BIZ.Domain.Entities;

public class Currency
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Symbol { get; set; }

    public string? Description { get; set; }

    public bool IsBaseCurrency { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<CurrencyRate> CurrencyRates { get; set; }
        = new List<CurrencyRate>();
}