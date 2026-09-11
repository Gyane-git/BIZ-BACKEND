namespace BIZ.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(
        int userId,
        string username,
        int companyId,
        string companyCode,
        string companyName,
        IEnumerable<string>? roles = null,
        IEnumerable<string>? permissions = null);
}
