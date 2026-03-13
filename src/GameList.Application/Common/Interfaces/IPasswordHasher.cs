namespace GameList.Application.Common.Interfaces;

/// <summary>
/// Servicio para generar y verificar hashes de contraseñas.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Genera un hash seguro de la contraseña proporcionada.
    /// </summary>
    /// <param name="password">Contraseña en texto plano.</param>
    /// <returns>Hash de la contraseña.</returns>
    string Hash(string password);

    /// <summary>
    /// Comprueba si la contraseña en texto plano coincide con el hash almacenado.
    /// </summary>
    /// <param name="password">Contraseña en texto plano a verificar.</param>
    /// <param name="hash">Hash almacenado contra el que comparar.</param>
    /// <returns><c>true</c> si la contraseña es correcta.</returns>
    bool Verify(string password, string hash);
}
