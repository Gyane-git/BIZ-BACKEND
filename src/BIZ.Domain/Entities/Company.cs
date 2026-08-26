namespace BIZ.Domain.Entities;

public class Company
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string DatabaseServer { get; set; } = string.Empty;

    public string DatabaseName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public string SubscriptionPlan { get; set; } = string.Empty;

    public DateTime? SubscriptionStart { get; set; }

    public DateTime? SubscriptionEnd { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}