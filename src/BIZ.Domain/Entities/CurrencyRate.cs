namespace BIZ.Domain.Entities;

public class CurrencyRate
{
    public int Id { get; set; }

    public int CurrencyId { get; set; }

    public DateTime RateDate { get; set; }

    public decimal BuyingRate { get; set; }

    public decimal SellingRate { get; set; }

    public decimal? AverageRate { get; set; }

    public string? Remarks { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Currency Currency { get; set; } = null!;
}