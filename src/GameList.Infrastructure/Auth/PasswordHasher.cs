using System.Security.Cryptography;
using GameList.Application.Common.Interfaces;

namespace GameList.Infrastructure.Auth;

/// <summary>
/// Implementación de hashing de contraseñas usando PBKDF2 con SHA-256 y salt aleatorio.
/// </summary>
internal sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16, KeySize = 32, Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    /// <summary>
    /// Genera un hash seguro de la contraseña con un salt aleatorio.
    /// El resultado tiene el formato "base64(salt):base64(hash)".
    /// </summary>
    /// <param name="password">Contraseña en texto plano.</param>
    /// <returns>Hash de la contraseña en formato "salt:hash" codificado en Base64.</returns>
    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(key)}";
    }

    /// <summary>
    /// Verifica que la contraseña en texto plano coincide con el hash almacenado.
    /// Usa comparación de tiempo constante para evitar ataques de tiempo.
    /// </summary>
    /// <param name="password">Contraseña en texto plano a verificar.</param>
    /// <param name="hash">Hash almacenado en formato "salt:hash" codificado en Base64.</param>
    /// <returns><c>true</c> si la contraseña es correcta; <c>false</c> en caso contrario.</returns>
    public bool Verify(string password, string hash)
    {
        var parts = hash.Split(':');
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
