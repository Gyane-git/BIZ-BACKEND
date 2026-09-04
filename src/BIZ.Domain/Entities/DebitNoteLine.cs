namespace BIZ.Domain.Entities;

public class DebitNoteLine
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

    public DebitNote DebitNote { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
