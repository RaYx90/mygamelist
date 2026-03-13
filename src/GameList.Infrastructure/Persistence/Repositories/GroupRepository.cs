using GameList.Domain.Entities;
using GameList.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameList.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación EF Core del puerto <see cref="IGroupRepository"/>.
/// Todos los métodos delegan en el DbContext; los cambios se persisten con SaveChangesAsync.
/// </summary>
internal sealed class GroupRepository : IGroupRepository
{
    private readonly AppDbContext context;
    public GroupRepository(AppDbContext context) => this.context = context;

    /// <summary>
    /// Carga el grupo junto con su colección Members (eager loading via Include).
    /// Se necesita la lista de miembros para construir el DTO en GetMyGroupHandler.
    /// </summary>
    public Task<GroupEntity?> GetByIdAsync(int id, CancellationToken ct) =>
        context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == id, ct);

    /// <summary>
    /// Busca por código de invitación sin cargar Members — solo se necesita el Id del grupo
    /// para asignarlo como FK en el usuario (JoinGroupHandler).
    /// </summary>
    public Task<GroupEntity?> GetByInviteCodeAsync(string inviteCode, CancellationToken ct) =>
        context.Groups.FirstOrDefaultAsync(g => g.InviteCode == inviteCode, ct);

    public async Task AddAsync(GroupEntity group, CancellationToken ct) =>
        await context.Groups.AddAsync(group, ct);

    public Task SaveChangesAsync(CancellationToken ct) => context.SaveChangesAsync(ct);
}
