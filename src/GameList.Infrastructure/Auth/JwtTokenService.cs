using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameList.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GameList.Infrastructure.Auth;

internal sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptionsConfig _options;
    public JwtTokenService(IOptions<JwtOptionsConfig> options) => _options = options.Value;

    public string GenerateToken(int userId, string username, string email, int? groupId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, email)
        };
        if (groupId.HasValue)
            claims.Add(new("groupId", groupId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_options.ExpiryDays),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
