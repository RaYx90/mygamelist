using GameList.Domain.Entities;
using GameList.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameList.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación EF Core del repositorio de usuarios.
/// </summary>
internal sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext context;

    /// <summary>
    /// Inicializa el repositorio con el contexto de base de datos.
    /// </summary>
    /// <param name="context">Contexto de EF Core.</param>
    public UserRepository(AppDbContext context) => this.context = context;

    /// <summary>
    /// Obtiene un usuario por su identificador interno.
    /// </summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El usuario encontrado, o <c>null</c> si no existe.</returns>
    public Task<UserEntity?> GetByIdAsync(int id, CancellationToken ct) =>
        context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    /// <summary>
    /// Obtiene un usuario por su correo electrónico (insensible a mayúsculas).
    /// </summary>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El usuario encontrado, o <c>null</c> si no existe.</returns>
    public Task<UserEntity?> GetByEmailAsync(string email, CancellationToken ct) =>
        context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    /// <summary>
    /// Comprueba si existe un usuario con el correo electrónico indicado.
    /// </summary>
    /// <param name="email">Correo a comprobar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns><c>true</c> si el correo ya está registrado.</returns>
    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        context.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

    /// <summary>
    /// Comprueba si existe un usuario con el nombre de usuario indicado.
    /// </summary>
    /// <param name="username">Nombre de usuario a comprobar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns><c>true</c> si el nombre de usuario ya está en uso.</returns>
    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct) =>
        context.Users.AnyAsync(u => u.Username == username.Trim(), ct);

    /// <summary>
    /// Devuelve todos los usuarios pertenecientes al grupo indicado.
    /// </summary>
    /// <param name="groupId">Identificador del grupo.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de usuarios del grupo.</returns>
    public async Task<IReadOnlyList<UserEntity>> GetByGroupIdAsync(int groupId, CancellationToken ct) =>
        await context.Users.Where(u => u.GroupId == groupId).ToListAsync(ct);

    /// <summary>
    /// Añade un nuevo usuario al contexto.
    /// </summary>
    /// <param name="user">Entidad del usuario a añadir.</param>
    /// <param name="ct">Token de cancelación.</param>
    public async Task AddAsync(UserEntity user, CancellationToken ct) =>
        await context.Users.AddAsync(user, ct);

    /// <summary>
    /// Marca un usuario como modificado en el contexto.
    /// </summary>
    /// <param name="user">Entidad del usuario a actualizar.</param>
    public void Update(UserEntity user) => context.Users.Update(user);

    /// <summary>
    /// Persiste los cambios pendientes en la base de datos.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    public Task SaveChangesAsync(CancellationToken ct) => context.SaveChangesAsync(ct);
}
