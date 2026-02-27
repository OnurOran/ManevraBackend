namespace MyApp.Api.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string HashedToken { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string hashedToken, DateTime expiresAt) =>
        new() { UserId = userId, HashedToken = hashedToken, ExpiresAt = expiresAt };

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired && !IsDeleted;

    public void Revoke() => IsRevoked = true;
}
