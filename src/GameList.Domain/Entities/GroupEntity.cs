namespace GameList.Domain.Entities;

/// <summary>
/// Representa un grupo social al que los usuarios pueden unirse para compartir sus listas de juegos.
/// Cada grupo tiene un código de invitación único de 8 caracteres alfanuméricos.
/// </summary>
public sealed class GroupEntity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Código corto para invitar a otros usuarios. Se genera en el momento de creación del grupo.
    /// Ver <c>CreateGroupHandler</c> para el algoritmo de generación.
    /// </summary>
    public string InviteCode { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    // Campo privado para evitar que código externo manipule la colección directamente.
    private readonly List<UserEntity> _members = [];

    /// <summary>
    /// Vista de solo lectura de los miembros del grupo.
    /// Solo se carga cuando se hace Include() explícito en las consultas EF Core.
    /// </summary>
    public IReadOnlyCollection<UserEntity> Members => _members.AsReadOnly();

    // Constructor privado requerido por EF Core para materializar entidades desde la BD.
    private GroupEntity() { }

    /// <summary>
    /// Método de fábrica que crea un <see cref="GroupEntity"/> válido.
    /// </summary>
    /// <param name="name">Nombre visible del grupo. Los espacios extremos se eliminan.</param>
    /// <param name="inviteCode">Código de invitación pregenerado (ver <c>CreateGroupHandler</c>).</param>
    public static GroupEntity Create(string name, string inviteCode)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Group name cannot be empty.", nameof(name));
        return new GroupEntity { Name = name.Trim(), InviteCode = inviteCode, CreatedAt = DateTime.UtcNow };
    }
}
