using GameList.Domain.Entities;

namespace GameList.Domain.Ports;

public interface IGroupRepository
{
    Task<GroupEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<GroupEntity?> GetByInviteCodeAsync(string inviteCode, CancellationToken cancellationToken = default);
    Task AddAsync(GroupEntity group, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
