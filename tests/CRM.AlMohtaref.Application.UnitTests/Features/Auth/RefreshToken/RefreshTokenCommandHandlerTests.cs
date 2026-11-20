using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Auth.Login;          // RefreshTokenDto lives here
using CRM.AlMohtaref.Application.Features.Auth.RefreshToken;   // RefreshTokenCommand
using CRM.AlMohtaref.Application.Fseatures.Auth.RefreshToken;  // handler (namespace typo "Fseatures" in production code)
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Users;
using Moq;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Auth.RefreshToken;

/// <summary>
/// Tests tagged Category=BugProbe are expected to FAIL on the current code.
/// </summary>
public class RefreshTokenCommandHandlerTests
{
    private const string Secret = "secret";
    private const string StoredHash = "stored-hash";

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUserRefreshTokenRepository> _refreshTokens = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private readonly User _user =
        new(Guid.NewGuid(), "acc", "pw-hash", new Person("Full Name", Guid.NewGuid()), "a@b.com");

    private UserRefreshToken? _added;

    public RefreshTokenCommandHandlerTests()
    {
        _users.Setup(u => u.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_user);

        _hasher.Setup(h => h.VerifyPassword(Secret, StoredHash)).Returns(true);
        _hasher.Setup(h => h.HashPassword(It.IsAny<string>())).Returns<string>(p => "H:" + p);

        _tokens.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).ReturnsAsync("new-jwt");
        _tokens.Setup(t => t.GenerateRefreshToken()).Returns("new-secret");
        _tokens.Setup(t => t.GetExpireAt()).Returns(new DateTime(2030, 1, 1));

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _refreshTokens.Setup(r => r.Add(It.IsAny<UserRefreshToken>()))
                      .Callback<UserRefreshToken>(t => _added = t);
    }

    private RefreshTokenCommandHandler Sut =>
        new(_users.Object, _refreshTokens.Object, _tokens.Object, _hasher.Object, _uow.Object);

    // NOTE: if IUserRefreshTokenRepository.GetById has a CancellationToken parameter, change it here only.
    private UserRefreshToken StoredToken(DateTime? expiry = null, DateTime? revokedAt = null)
    {
        var stored = new UserRefreshToken(Guid.NewGuid(), _user.Id, StoredHash,
            expiry ?? DateTime.UtcNow.AddDays(1), revokedAt);
        _refreshTokens.Setup(r => r.GetById(stored.Id, CancellationToken.None)).ReturnsAsync(stored);
        return stored;
    }

    private static string Raw(UserRefreshToken stored, string secret = Secret) => $"{stored.Id}.{secret}";

    private Task<RefreshTokenDto> Refresh(string raw, CancellationToken ct = default) =>
        Sut.Handle(new RefreshTokenCommand(raw), ct);

    private void VerifyNothingIssued()
    {
        _refreshTokens.Verify(r => r.Add(It.IsAny<UserRefreshToken>()), Times.Never());
        _tokens.Verify(t => t.GenerateAccessToken(It.IsAny<User>()), Times.Never());
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    // ------------------------------------------------------------ happy path

    [Fact]
    public async Task ValidToken_ReturnsNewTokensAndUserInfo()
    {
        var stored = StoredToken();

        var result = await Refresh(Raw(stored));

        Assert.Equal("new-jwt", result.Token);
        Assert.Equal(new DateTime(2030, 1, 1), result.ExpiresIn);
        Assert.Equal(_user.Id, result.UserId);
        Assert.Equal("Full Name", result.FullName);
        Assert.Empty(result.Permissions);
    }

    [Fact]
    public async Task ValidToken_IsRotated_OldRevoked_NewStoredAndSaved()
    {
        var stored = StoredToken();
        var before = DateTime.UtcNow;

        var result = await Refresh(Raw(stored));

        Assert.NotNull(stored.RefreshTokenRevokedAt);
        Assert.InRange(stored.RefreshTokenRevokedAt!.Value, before, DateTime.UtcNow);

        Assert.NotNull(_added);
        Assert.NotEqual(stored.Id, _added!.Id);
        Assert.Equal(_user.Id, _added.UserId);
        Assert.Equal("H:new-secret", _added.RefreshTokenHash);
        Assert.Null(_added.RefreshTokenRevokedAt);
        Assert.Equal($"{_added.Id}.new-secret", result.RefreshToken);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task NewRefreshToken_ExpiresInSevenDays()
    {
        var stored = StoredToken();
        var before = DateTime.UtcNow;

        await Refresh(Raw(stored));

        Assert.InRange(_added!.RefreshTokenExpiryTime, before.AddDays(7), DateTime.UtcNow.AddDays(7));
    }

    [Fact]
    public async Task Save_ReceivesTheCancellationToken()
    {
        var stored = StoredToken();
        using var cts = new CancellationTokenSource();

        await Refresh(Raw(stored), cts.Token);

        _uow.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once());
    }

    // ------------------------------------------------------------ malformed input

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("a.b.c")]
    public async Task WrongNumberOfParts_ThrowsBadRequest_AndTouchesNoRepository(string raw)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => Refresh(raw));

        _refreshTokens.Verify(r => r.GetById(It.IsAny<Guid>(), CancellationToken.None), Times.Never());
        VerifyNothingIssued();
    }

    // ------------------------------------------------------------ rejected tokens

    [Fact]
    public async Task UnknownToken_ThrowsUnauthorized()
    {
        var id = Guid.NewGuid();
        _refreshTokens.Setup(r => r.GetById(id, CancellationToken.None)).ReturnsAsync((UserRefreshToken?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh($"{id}.{Secret}"));

        VerifyNothingIssued();
    }

    [Fact]
    public async Task ExpiredToken_ThrowsUnauthorized_AndIssuesNothing()
    {
        var stored = StoredToken(expiry: DateTime.UtcNow.AddSeconds(-1));

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(Raw(stored)));

        VerifyNothingIssued();
    }

    [Fact]
    public async Task WrongSecret_ThrowsUnauthorized_AndDoesNotRotate()
    {
        var stored = StoredToken();

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(Raw(stored, "wrong")));

        Assert.Null(stored.RefreshTokenRevokedAt);
        VerifyNothingIssued();
    }

    [Fact]
    public async Task RevokedToken_IsRejected_AndAllUserTokensAreRevoked()
    {
        var stored = StoredToken(revokedAt: DateTime.UtcNow.AddMinutes(-5));

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(Raw(stored)));

        _refreshTokens.Verify(r => r.RevokeUserTokens(_user.Id, It.IsAny<CancellationToken>()), Times.Once());
        VerifyNothingIssued();
    }

    [Fact]
    public async Task ReplayingARotatedToken_IsRejected()
    {
        var stored = StoredToken();
        var raw = Raw(stored);

        await Refresh(raw);                                              // first use: OK
        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(raw)); // second use: reuse detected

        _refreshTokens.Verify(r => r.RevokeUserTokens(_user.Id, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task UserNoLongerExists_IssuesNothing()
    {
        var stored = StoredToken();
        _users.Setup(u => u.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => Refresh(Raw(stored)));

        VerifyNothingIssued();
    }

    [Fact]
    public async Task AccessTokenFailure_DoesNotSave()
    {
        var stored = StoredToken();
        _tokens.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).ThrowsAsync(new InvalidOperationException("jwt"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Refresh(Raw(stored)));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    // ------------------------------------------------------------ BUG PROBES (expected to FAIL today)

    [Theory]
    [Trait("Category", "BugProbe")]
    [InlineData("not-a-guid.secret")]
    [InlineData(".secret")]
    [InlineData(".")]
    public async Task NonGuidTokenId_ShouldBeBadRequest_NotFormatException(string raw)
    {
        // Guid.Parse throws FormatException -> surfaces as HTTP 500 instead of 400.
        await Assert.ThrowsAsync<BadRequestException>(() => Refresh(raw));
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public async Task NullToken_ShouldBeBadRequest_NotNullReference()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => Refresh(null!));
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public async Task InactiveUser_ShouldNotBeAbleToRefresh()
    {
        // Login rejects inactive users, but Refresh never checks IsActive.
        var stored = StoredToken();
        _user.UpdateProfile("acc", "a@b.com", false, "Full Name");

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(Raw(stored)));

        VerifyNothingIssued();
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public async Task LockedOutUser_ShouldNotBeAbleToRefresh()
    {
        var stored = StoredToken();
        _user.LockAccount(DateTime.UtcNow.AddDays(1));

        await Assert.ThrowsAsync<ForbiddenException>(() => Refresh(Raw(stored)));

        VerifyNothingIssued();
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public async Task RevokedToken_WithWrongSecret_ShouldNotRevokeAllUserTokens()
    {
        // The secret is verified AFTER the revoked check, so anyone who only knows a token ID (a GUID)
        // can force-logout the owner everywhere.
        var stored = StoredToken(revokedAt: DateTime.UtcNow.AddMinutes(-5));

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(Raw(stored, "attacker-guess")));

        _refreshTokens.Verify(r => r.RevokeUserTokens(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public async Task ReusedToken_ThatIsAlsoExpired_ShouldStillTriggerReuseDetection()
    {
        // The expiry check runs first, so reuse of an old rotated token goes undetected.
        var stored = StoredToken(expiry: DateTime.UtcNow.AddDays(-1), revokedAt: DateTime.UtcNow.AddDays(-2));

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(Raw(stored)));

        _refreshTokens.Verify(r => r.RevokeUserTokens(_user.Id, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public async Task ShouldForwardCancellationTokenToUserRepository()
    {
        var stored = StoredToken();
        using var cts = new CancellationTokenSource();

        await Refresh(Raw(stored), cts.Token);

        _users.Verify(u => u.GetByIdAsync(_user.Id, cts.Token), Times.Once());
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public async Task ShouldForwardCancellationTokenWhenRevokingAllTokens()
    {
        var stored = StoredToken(revokedAt: DateTime.UtcNow.AddMinutes(-1));
        using var cts = new CancellationTokenSource();

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(Raw(stored), cts.Token));

        _refreshTokens.Verify(r => r.RevokeUserTokens(_user.Id, cts.Token), Times.Once());
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public async Task UnknownUser_ShouldBeUnauthorized_NotNotFound()
    {
        // 404 for a deleted user leaks account state through the auth endpoint.
        var stored = StoredToken();
        _users.Setup(u => u.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(Raw(stored)));
    }
}
