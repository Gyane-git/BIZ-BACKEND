namespace BIZ.Application.DTOs;

public class CurrencyRateDto
{
    public int Id { get; set; }

    public int CurrencyId { get; set; }

    public DateTime RateDate { get; set; }

    public decimal BuyingRate { get; set; }

    public decimal SellingRate { get; set; }

    public decimal? AverageRate { get; set; }

    public string? Remarks { get; set; }

    public bool IsActive { get; set; } = true;
}