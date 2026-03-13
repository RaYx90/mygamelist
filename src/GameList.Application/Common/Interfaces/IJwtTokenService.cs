namespace GameList.Application.Common.Interfaces;

/// <summary>
/// Servicio para generar tokens JWT de autenticación.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Genera un token JWT firmado con los datos del usuario proporcionados.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="username">Nombre de usuario.</param>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <param name="groupId">Identificador del grupo al que pertenece el usuario (opcional).</param>
    /// <returns>Token JWT serializado como cadena.</returns>
    string GenerateToken(int userId, string username, string email, int? groupId);
}
