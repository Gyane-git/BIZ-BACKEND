namespace BIZ.Application.DTOs;

public class DebitNoteLineDto
{
    public int Id { get; set; }

    public int DebitNoteId { get; set; }

    public int ProductId { get; set; }

    public string? Description { get; set; }

    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }

    public decimal DiscountAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }

    public decimal LineTotal { get; set; }

    public int LineNumber { get; set; }
}