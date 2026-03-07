namespace GameList.Domain.Entities;

public sealed class UserEntity
{
    public int Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public int? GroupId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public GroupEntity? Group { get; private set; }
    private readonly List<GameFavoriteEntity> _favorites = [];
    private readonly List<GamePurchaseEntity> _purchases = [];
    public IReadOnlyCollection<GameFavoriteEntity> Favorites => _favorites.AsReadOnly();
    public IReadOnlyCollection<GamePurchaseEntity> Purchases => _purchases.AsReadOnly();

    private UserEntity() { }

    public static UserEntity Create(string username, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username cannot be empty.", nameof(username));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
        return new UserEntity { Username = username.Trim(), Email = email.Trim().ToLowerInvariant(), PasswordHash = passwordHash, CreatedAt = DateTime.UtcNow };
    }

    public void JoinGroup(int groupId) => GroupId = groupId;
    public void LeaveGroup() => GroupId = null;
}
