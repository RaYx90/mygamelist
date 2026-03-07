using GameList.Domain.Entities;
using GameList.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace GameList.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context) => _context = context;

    public Task<UserEntity?> GetByIdAsync(int id, CancellationToken ct) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<UserEntity?> GetByEmailAsync(string email, CancellationToken ct) =>
        _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        _context.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct) =>
        _context.Users.AnyAsync(u => u.Username == username.Trim(), ct);

    public async Task<IReadOnlyList<UserEntity>> GetByGroupIdAsync(int groupId, CancellationToken ct) =>
        await _context.Users.Where(u => u.GroupId == groupId).ToListAsync(ct);

    public async Task AddAsync(UserEntity user, CancellationToken ct) =>
        await _context.Users.AddAsync(user, ct);

    public void Update(UserEntity user) => _context.Users.Update(user);

    public Task SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}
