namespace BIZ.Application.DTOs;

public class AccountSubGroupDto
{
    public int Id { get; set; }

    public int AccountGroupId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}