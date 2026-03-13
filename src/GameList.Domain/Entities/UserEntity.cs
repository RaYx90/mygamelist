namespace GameList.Domain.Entities;

/// <summary>
/// Representa un usuario de la aplicación con credenciales de autenticación y membresía opcional en un grupo.
/// Un usuario puede marcar juegos como favoritos o como comprados, y pertenecer a un solo grupo social.
/// </summary>
public sealed class UserEntity
{
    public int Id { get; private set; }
    public string Username { get; private set; } = string.Empty;

    /// <summary>
    /// Almacenado en minúsculas (cultura invariante) para simplificar búsquedas sin distinguir mayúsculas.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Hash de la contraseña (BCrypt). La contraseña en texto plano nunca se almacena.
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Clave foránea al grupo actual del usuario. <c>null</c> cuando no pertenece a ningún grupo.
    /// Se modifica únicamente a través de <see cref="JoinGroup"/> y <see cref="LeaveGroup"/>.
    /// </summary>
    public int? GroupId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Propiedad de navegación al grupo. Solo se carga cuando se hace Include() explícito.
    /// </summary>
    public GroupEntity? Group { get; private set; }

    // Campos privados para evitar que código externo manipule las colecciones directamente.
    private readonly List<GameFavoriteEntity> favorites = [];
    private readonly List<GamePurchaseEntity> purchases = [];

    public IReadOnlyCollection<GameFavoriteEntity> Favorites => favorites.AsReadOnly();
    public IReadOnlyCollection<GamePurchaseEntity> Purchases => purchases.AsReadOnly();

    // Constructor privado requerido por EF Core para materializar entidades desde la BD.
    private UserEntity() { }

    /// <summary>
    /// Método de fábrica que crea un <see cref="UserEntity"/> válido.
    /// El email se normaliza a minúsculas para que las búsquedas sean siempre insensibles a mayúsculas.
    /// </summary>
    public static UserEntity Create(string username, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username cannot be empty.", nameof(username));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
        return new UserEntity { Username = username.Trim(), Email = email.Trim().ToLowerInvariant(), PasswordHash = passwordHash, CreatedAt = DateTime.UtcNow };
    }

    /// <summary>Asigna el usuario al grupo indicado guardando su clave primaria.</summary>
    public void JoinGroup(int groupId) => GroupId = groupId;

    /// <summary>Desvincula al usuario de su grupo actual.</summary>
    public void LeaveGroup() => GroupId = null;
}
