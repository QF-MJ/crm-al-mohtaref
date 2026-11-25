using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Auth;
using CRM.AlMohtaref.Application.Features.Auth.Login;
using CRM.AlMohtaref.Application.Features.Auth.RefreshToken;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Application.Interfaces.Common;
using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Contacts;
using CRM.AlMohtaref.Domain.Entities.Users;
using Moq;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Auth;

public class RefreshTokenCommandHandlerTests
{
    private const string OldSecret = "old-secret";
    private const string OldHash = "old-hash";
    private const string NewRawSecret = "new-raw";
    private const string NewHash = "new-hash";

    private static readonly Guid OldTokenId = Guid.NewGuid();

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUserRefreshTokenRepository> _refreshTokens = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private readonly List<string> _calls = new();
    private UserRefreshToken? _addedRefreshToken;
    private UserRefreshToken _oldToken;

    private readonly User _user;
    private readonly RefreshTokenCommandHandler _handler;

    private static string OldTokenString => $"{OldTokenId}.{OldSecret}";

    public RefreshTokenCommandHandlerTests()
    {
        _user = new User(Guid.NewGuid(), "acc", "pwd-hash", new Person("Full Name", Guid.NewGuid()), "a@b.com");

        _users.Setup(u => u.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_user);

        _oldToken = StoredToken();
        SetStoredToken(_oldToken);

        _refreshTokens.Setup(r => r.Add(It.IsAny<UserRefreshToken>()))
                      .Callback<UserRefreshToken>(t =>
                      {
                          _calls.Add("add");
                          _addedRefreshToken = t;
                      });

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("save"))
            .ReturnsAsync(1);

        _hasher.Setup(h => h.VerifyPassword(OldSecret, OldHash)).Returns(true);
        _hasher.Setup(h => h.HashPassword(NewRawSecret)).Returns(NewHash);

        _tokens.Setup(t => t.GenerateAccessToken(_user)).ReturnsAsync("jwt");
        _tokens.Setup(t => t.GenerateRefreshToken()).Returns(NewRawSecret);
        _tokens.Setup(t => t.GetExpireAt()).Returns(new DateTime(2030, 1, 1));

        _handler = new RefreshTokenCommandHandler(
            _users.Object, _refreshTokens.Object, _tokens.Object, _hasher.Object, _uow.Object);
    }

    private UserRefreshToken StoredToken(DateTime? expiresAt = null, DateTime? revokedAt = null) =>
        new(OldTokenId, _user.Id, OldHash, expiresAt ?? DateTime.UtcNow.AddDays(7), revokedAt);

    private void SetStoredToken(UserRefreshToken? token)
    {
        _oldToken = token!;
        _refreshTokens.Setup(r => r.GetById(OldTokenId, It.IsAny<CancellationToken>())).ReturnsAsync(token);
    }

    private Task<RefreshTokenDto> Refresh(string token, CancellationToken ct = default) =>
        _handler.Handle(new RefreshTokenCommand(token), ct);

    private void VerifyNothingIssued()
    {
        _tokens.Verify(t => t.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        _tokens.Verify(t => t.GenerateRefreshToken(), Times.Never);
        _refreshTokens.Verify(r => r.Add(It.IsAny<UserRefreshToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------------------------------------------------------------- rejected requests

    [Theory]
    [InlineData("")]
    [InlineData("no-separator")]
    [InlineData("a.b.c")]
    public async Task InvalidFormat_ThrowsBadRequest_AndIssuesNothing(string token)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => Refresh(token));

        _refreshTokens.Verify(r => r.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        VerifyNothingIssued();
    }

    [Fact]
    public async Task UnknownToken_ThrowsUnauthorized_AndIssuesNothing()
    {
        SetStoredToken(null);

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(OldTokenString));

        VerifyNothingIssued();
    }

    [Fact]
    public async Task UserNotFound_ThrowsNotFound_AndIssuesNothing()
    {
        _users.Setup(u => u.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => Refresh(OldTokenString));

        VerifyNothingIssued();
    }

    [Fact]
    public async Task ExpiredToken_ThrowsUnauthorized_AndIssuesNothing()
    {
        SetStoredToken(StoredToken(expiresAt: DateTime.UtcNow.AddMinutes(-1)));

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(OldTokenString));

        Assert.Null(_oldToken.RefreshTokenRevokedAt);
        VerifyNothingIssued();
    }

    [Fact]
    public async Task AlreadyRevokedToken_ThrowsUnauthorized_AndIssuesNothing()
    {
        SetStoredToken(StoredToken(revokedAt: DateTime.UtcNow.AddMinutes(-5)));

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(OldTokenString));

        VerifyNothingIssued();
    }

    // إعادة استخدام توكن ملغى = احتمال سرقة، فيجب إلغاء كل توكنات المستخدم
    [Fact]
    public async Task AlreadyRevokedToken_RevokesAllTokensOfTheUser()
    {
        SetStoredToken(StoredToken(revokedAt: DateTime.UtcNow.AddMinutes(-5)));

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(OldTokenString));

        _refreshTokens.Verify(
            r => r.RevokeUserTokens(_user.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidToken_DoesNotRevokeAllUserTokens()
    {
        await Refresh(OldTokenString);

        _refreshTokens.Verify(
            r => r.RevokeUserTokens(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WrongSecret_ThrowsUnauthorized_AndDoesNotRevokeOrIssue()
    {
        _hasher.Setup(h => h.VerifyPassword("wrong", OldHash)).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh($"{OldTokenId}.wrong"));

        Assert.Null(_oldToken.RefreshTokenRevokedAt);
        VerifyNothingIssued();
    }

    // ---------------------------------------------------------------- successful refresh

    [Fact]
    public async Task ValidToken_ReturnsNewAccessTokenAndUserInfo()
    {
        var result = await Refresh(OldTokenString);

        Assert.Equal("jwt", result.Token);
        Assert.Equal(_user.Id, result.UserId);
        Assert.Equal(new DateTime(2030, 1, 1), result.ExpiresIn);
        Assert.Empty(result.Permissions);
        _tokens.Verify(t => t.GenerateAccessToken(_user), Times.Once);
    }

    [Fact]
    public async Task ValidToken_RevokesTheOldToken()
    {
        Assert.Null(_oldToken.RefreshTokenRevokedAt);

        await Refresh(OldTokenString);

        Assert.NotNull(_oldToken.RefreshTokenRevokedAt);
    }

    [Fact]
    public async Task ValidToken_ReturnsNewRefreshToken_AsIdDotSecret()
    {
        var result = await Refresh(OldTokenString);

        Assert.NotNull(result.RefreshToken);
        var separator = result.RefreshToken!.IndexOf('.');
        Assert.True(separator > 0, "refresh token must look like '<guid>.<secret>'");
        Assert.True(Guid.TryParse(result.RefreshToken[..separator], out var id));
        Assert.NotEqual(Guid.Empty, id);
        Assert.NotEqual(OldTokenId, id);
        Assert.Equal(NewRawSecret, result.RefreshToken[(separator + 1)..]);
    }

    [Fact]
    public async Task ValidToken_StoresExactlyOneNewRefreshToken_ForTheSameUser()
    {
        await Refresh(OldTokenString);

        _refreshTokens.Verify(r => r.Add(It.IsAny<UserRefreshToken>()), Times.Once);
        Assert.NotNull(_addedRefreshToken);
        Assert.Equal(_user.Id, _addedRefreshToken!.UserId);
        Assert.NotEqual(OldTokenId, _addedRefreshToken.Id);
        Assert.Null(_addedRefreshToken.RefreshTokenRevokedAt);
    }

    [Fact]
    public async Task StoredRefreshToken_IsHashed_AndDoesNotContainThePlainSecret()
    {
        var result = await Refresh(OldTokenString);

        Assert.Equal(NewHash, _addedRefreshToken!.RefreshTokenHash);
        Assert.NotEqual(NewRawSecret, _addedRefreshToken.RefreshTokenHash);
        Assert.NotEqual(result.RefreshToken, _addedRefreshToken.RefreshTokenHash);
    }

    [Fact]
    public async Task StoredRefreshToken_ExpiresInSevenDays()
    {
        await Refresh(OldTokenString);

        var expected = DateTime.UtcNow.AddDays(7);
        Assert.InRange(
            _addedRefreshToken!.RefreshTokenExpiryTime,
            expected.AddMinutes(-1),
            expected.AddMinutes(1));
    }

    [Fact]
    public async Task ReturnedRefreshToken_MatchesTheStoredTokenId()
    {
        var result = await Refresh(OldTokenString);

        var returnedId = Guid.Parse(result.RefreshToken!.Split('.')[0]);
        Assert.Equal(_addedRefreshToken!.Id, returnedId);
    }

    // ---------------------------------------------------------------- persistence

    [Fact]
    public async Task ValidToken_SavesOnce_AfterAddingTheNewToken()
    {
        await Refresh(OldTokenString);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(new[] { "add", "save" }, _calls);
    }

    // ---------------------------------------------------------------- cancellation

    [Fact]
    public async Task CancellationToken_IsPassedToSaveChanges()
    {
        using var cts = new CancellationTokenSource();

        await Refresh(OldTokenString, cts.Token);

        _uow.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }
}