namespace BIZ.Domain.Entities;

public class LoginHistory
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int? CompanyId { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime LoginAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? FailureReason { get; set; }
    public User? User { get; set; }
    public Company? Company { get; set; }
}
