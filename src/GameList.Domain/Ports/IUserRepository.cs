using GameList.Domain.Entities;

namespace GameList.Domain.Ports;

public interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserEntity>> GetByGroupIdAsync(int groupId, CancellationToken cancellationToken = default);
    Task AddAsync(UserEntity user, CancellationToken cancellationToken = default);
    void Update(UserEntity user);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
