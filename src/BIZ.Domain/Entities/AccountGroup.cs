namespace BIZ.Domain.Entities;

public class AccountGroup
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Nature { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<AccountSubGroup> AccountSubGroups { get; set; }
        = new List<AccountSubGroup>();
}