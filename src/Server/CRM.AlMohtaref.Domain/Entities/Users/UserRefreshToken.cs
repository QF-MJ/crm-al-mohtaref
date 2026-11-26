namespace CRM.AlMohtaref.Domain.Entities.Users;

public class UserRefreshToken
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string? RefreshTokenHash { get; private set; }

    public DateTime RefreshTokenExpiryTime { get; private set; }

    public DateTime? RefreshTokenRevokedAt { get; private set; }

    public User User { get; private set; }

    private UserRefreshToken()
    {
        User = null!;
    }

    public UserRefreshToken(Guid id, Guid userId, string? refreshTokenHash, DateTime refreshTokenExpiryTime, DateTime? refreshTokenRevokedAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("معرّف التوكن لا يمكن أن يكون فارغاً", nameof(id));

        if (userId == Guid.Empty)
            throw new ArgumentException("معرّف المستخدم لا يمكن أن يكون فارغاً", nameof(userId));

        if (string.IsNullOrWhiteSpace(refreshTokenHash))
            throw new ArgumentException("قيمة التوكن المشفرة لا يمكن أن تكون فارغة", nameof(refreshTokenHash));

        if (refreshTokenExpiryTime == default)
            throw new ArgumentException("تاريخ انتهاء صلاحية التوكن غير صالح", nameof(refreshTokenExpiryTime));

        Id = id;
        UserId = userId;
        RefreshTokenHash = refreshTokenHash;
        RefreshTokenExpiryTime = refreshTokenExpiryTime;
        RefreshTokenRevokedAt = refreshTokenRevokedAt;
        User = null!;
    }

    public void RevokeToken(DateTime date)
    {
        if (date == default)
            throw new ArgumentException("تاريخ إلغاء التوكن غير صالح", nameof(date));

        if (RefreshTokenRevokedAt.HasValue)
            return;

        RefreshTokenRevokedAt = date;
    }

}
