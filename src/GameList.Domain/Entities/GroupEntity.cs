namespace GameList.Domain.Entities;

public sealed class GroupEntity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string InviteCode { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private readonly List<UserEntity> _members = [];
    public IReadOnlyCollection<UserEntity> Members => _members.AsReadOnly();

    private GroupEntity() { }

    public static GroupEntity Create(string name, string inviteCode)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Group name cannot be empty.", nameof(name));
        return new GroupEntity { Name = name.Trim(), InviteCode = inviteCode, CreatedAt = DateTime.UtcNow };
    }
}
