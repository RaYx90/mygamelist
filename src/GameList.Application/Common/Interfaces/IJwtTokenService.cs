namespace GameList.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(int userId, string username, string email, int? groupId);
}
