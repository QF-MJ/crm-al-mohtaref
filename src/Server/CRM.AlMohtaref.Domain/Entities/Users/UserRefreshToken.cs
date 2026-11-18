

namespace CRM.AlMohtaref.Domain.Entities.Users
{
    public class UserRefreshToken
    {
        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        public string? RefreshTokenHash { get; private set; }

        public DateTime RefreshTokenExpiryTime { get; private set; }

        public DateTime? RefreshTokenRevokedAt { get; set; }

        public User User { get; private set; }

        public UserRefreshToken(Guid id, Guid userId, string? refreshTokenHash, DateTime refreshTokenExpiryTime, DateTime? refreshTokenRevokedAt)
        {
            Id = id;
            UserId = userId;
            RefreshTokenHash = refreshTokenHash;
            RefreshTokenExpiryTime = refreshTokenExpiryTime;
            RefreshTokenRevokedAt = refreshTokenRevokedAt;
        }
    }
}
