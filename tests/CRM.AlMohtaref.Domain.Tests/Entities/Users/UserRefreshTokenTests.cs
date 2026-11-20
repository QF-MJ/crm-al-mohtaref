using System;
using System.Reflection;
using CRM.AlMohtaref.Domain.Entities.Users;
using Xunit;

namespace CRM.AlMohtaref.Domain.Tests.Entities.Users;

public class UserRefreshTokenTests
{
    private static UserRefreshToken NewToken(DateTime? revokedAt = null) =>
        new(Guid.NewGuid(), Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(7), revokedAt);

    [Fact]
    public void Constructor_StoresAllValues()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expiry = DateTime.UtcNow.AddDays(7);

        var token = new UserRefreshToken(id, userId, "hash", expiry, null);

        Assert.Equal(id, token.Id);
        Assert.Equal(userId, token.UserId);
        Assert.Equal("hash", token.RefreshTokenHash);
        Assert.Equal(expiry, token.RefreshTokenExpiryTime);
        Assert.Null(token.RefreshTokenRevokedAt);
    }

    [Fact]
    public void Constructor_CanCreateAnAlreadyRevokedToken()
    {
        var revoked = DateTime.UtcNow.AddDays(-1);

        Assert.Equal(revoked, NewToken(revoked).RefreshTokenRevokedAt);
    }

    [Fact]
    public void RevokeToken_SetsTheRevocationDate()
    {
        var token = NewToken();
        var when = DateTime.UtcNow;

        token.RevokeToken(when);

        Assert.Equal(when, token.RefreshTokenRevokedAt);
    }

    [Fact]
    public void RevokeToken_DoesNotTouchOtherFields()
    {
        var token = NewToken();
        var (id, userId, hash, expiry) = (token.Id, token.UserId, token.RefreshTokenHash, token.RefreshTokenExpiryTime);

        token.RevokeToken(DateTime.UtcNow);

        Assert.Equal(id, token.Id);
        Assert.Equal(userId, token.UserId);
        Assert.Equal(hash, token.RefreshTokenHash);
        Assert.Equal(expiry, token.RefreshTokenExpiryTime);
    }

    // ------------------------------------------------------------ BUG PROBES (expected to FAIL today)

    [Fact]
    [Trait("Category", "BugProbe")]
    public void RevokeToken_CalledTwice_ShouldKeepTheFirstRevocationDate()
    {
        // Revoking is overwritten silently: the audit trail of "when was it first revoked" is lost.
        var token = NewToken();
        var first = DateTime.UtcNow.AddHours(-3);
        var second = DateTime.UtcNow;

        token.RevokeToken(first);
        token.RevokeToken(second);

        Assert.Equal(first, token.RefreshTokenRevokedAt);
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public void RefreshTokenRevokedAt_ShouldNotHaveAPublicSetter()
    {
        // The public setter lets any layer un-revoke a token (set it back to null), bypassing RevokeToken.
        var setter = typeof(UserRefreshToken).GetProperty(nameof(UserRefreshToken.RefreshTokenRevokedAt))!.SetMethod;

        Assert.True(setter is null || !setter.IsPublic, "RefreshTokenRevokedAt has a public setter");
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public void Constructor_EmptyId_ShouldBeRejected() =>
        Assert.ThrowsAny<ArgumentException>(
            () => new UserRefreshToken(Guid.Empty, Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(1), null));

    [Fact]
    [Trait("Category", "BugProbe")]
    public void Constructor_EmptyUserId_ShouldBeRejected() =>
        Assert.ThrowsAny<ArgumentException>(
            () => new UserRefreshToken(Guid.NewGuid(), Guid.Empty, "hash", DateTime.UtcNow.AddDays(1), null));

    [Theory]
    [Trait("Category", "BugProbe")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_BlankHash_ShouldBeRejected(string? hash) =>
        // A token stored without a hash can never be validated (and VerifyPassword(secret, null) may misbehave).
        Assert.ThrowsAny<ArgumentException>(
            () => new UserRefreshToken(Guid.NewGuid(), Guid.NewGuid(), hash, DateTime.UtcNow.AddDays(1), null));
}
