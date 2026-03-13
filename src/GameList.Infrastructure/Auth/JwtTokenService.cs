using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameList.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GameList.Infrastructure.Auth;

/// <summary>
/// Implementación del servicio de generación de tokens JWT usando HMAC-SHA256.
/// </summary>
internal sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptionsConfig options;

    /// <summary>
    /// Inicializa el servicio con las opciones de JWT.
    /// </summary>
    /// <param name="options">Opciones de configuración JWT.</param>
    public JwtTokenService(IOptions<JwtOptionsConfig> options) => this.options = options.Value;

    /// <summary>
    /// Genera un token JWT con los claims del usuario, firmado con la clave secreta configurada.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="username">Nombre de usuario.</param>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <param name="groupId">Identificador del grupo del usuario (opcional).</param>
    /// <returns>Token JWT serializado como cadena.</returns>
    public string GenerateToken(int userId, string username, string email, int? groupId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, email)
        };
        if (groupId.HasValue)
            claims.Add(new("groupId", groupId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(options.ExpiryDays),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
