using GameList.Domain.Entities;

namespace GameList.Domain.Interfaces;

/// <summary>
/// Puerto de persistencia para <see cref="GroupEntity"/>.
/// Definido en la capa Domain para que Application dependa de la abstracción y no de EF Core.
/// La implementación concreta es <c>GroupRepository</c> en la capa Infrastructure.
/// </summary>
public interface IGroupRepository
{
    /// <summary>
    /// Obtiene un grupo por su clave primaria incluyendo la colección <c>Members</c> (eager loading).
    /// Devuelve <c>null</c> si no existe ningún grupo con ese id.
    /// </summary>
    Task<GroupEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca un grupo por su código de invitación. Se usa en el flujo de unirse a un grupo.
    /// No carga la colección <c>Members</c> — solo se necesita la identidad del grupo
    /// para validar el código y asignar la FK en el usuario.
    /// </summary>
    Task<GroupEntity?> GetByInviteCodeAsync(string inviteCode, CancellationToken cancellationToken = default);

    /// <summary>Registra un nuevo grupo para inserción. Requiere llamar a <see cref="SaveChangesAsync"/> para persistir.</summary>
    Task AddAsync(GroupEntity group, CancellationToken cancellationToken = default);

    /// <summary>Persiste todos los cambios pendientes en la base de datos.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
