namespace BIZ.Application.DTOs;

public class AgentDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? PanNumber { get; set; }

    public string? ContactPerson { get; set; }

    public decimal CommissionRate { get; set; }

    public bool IsActive { get; set; } = true;
}