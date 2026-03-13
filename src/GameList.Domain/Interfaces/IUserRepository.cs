using GameList.Domain.Entities;

namespace GameList.Domain.Interfaces;

/// <summary>
/// Puerto de dominio para el acceso y persistencia de usuarios.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Obtiene un usuario por su identificador interno.
    /// </summary>
    /// <param name="id">Identificador interno del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El usuario encontrado, o <c>null</c> si no existe.</returns>
    Task<UserEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un usuario por su dirección de correo electrónico.
    /// </summary>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El usuario encontrado, o <c>null</c> si no existe.</returns>
    Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Comprueba si ya existe un usuario registrado con el correo indicado.
    /// </summary>
    /// <param name="email">Correo electrónico a comprobar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns><c>true</c> si el correo ya está registrado.</returns>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Comprueba si ya existe un usuario con el nombre de usuario indicado.
    /// </summary>
    /// <param name="username">Nombre de usuario a comprobar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns><c>true</c> si el nombre de usuario ya está en uso.</returns>
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve todos los usuarios pertenecientes a un grupo.
    /// </summary>
    /// <param name="groupId">Identificador del grupo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de usuarios del grupo.</returns>
    Task<IReadOnlyList<UserEntity>> GetByGroupIdAsync(int groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Añade un nuevo usuario al contexto de persistencia.
    /// </summary>
    /// <param name="user">Entidad del usuario a añadir.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task AddAsync(UserEntity user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca un usuario como modificado en el contexto de persistencia.
    /// </summary>
    /// <param name="user">Entidad del usuario a actualizar.</param>
    void Update(UserEntity user);

    /// <summary>
    /// Persiste todos los cambios pendientes en la base de datos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
