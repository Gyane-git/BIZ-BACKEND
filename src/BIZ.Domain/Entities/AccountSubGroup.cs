namespace BIZ.Domain.Entities;

public class AccountSubGroup
{
    public int Id { get; set; }

    public int AccountGroupId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public AccountGroup AccountGroup { get; set; } = null!;
}