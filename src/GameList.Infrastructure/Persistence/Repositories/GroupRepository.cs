using GameList.Domain.Entities;
using GameList.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace GameList.Infrastructure.Persistence.Repositories;

internal sealed class GroupRepository : IGroupRepository
{
    private readonly AppDbContext _context;
    public GroupRepository(AppDbContext context) => _context = context;

    public Task<GroupEntity?> GetByIdAsync(int id, CancellationToken ct) =>
        _context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == id, ct);

    public Task<GroupEntity?> GetByInviteCodeAsync(string inviteCode, CancellationToken ct) =>
        _context.Groups.FirstOrDefaultAsync(g => g.InviteCode == inviteCode, ct);

    public async Task AddAsync(GroupEntity group, CancellationToken ct) =>
        await _context.Groups.AddAsync(group, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}
