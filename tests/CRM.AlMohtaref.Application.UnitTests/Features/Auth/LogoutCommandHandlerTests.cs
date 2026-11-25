using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Auth.Logout;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Users;
using Moq;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Auth;

public class LogOutCommandHandlerTests
{
    private const string Secret = "raw-secret";
    private const string StoredHash = "stored-hash";

    private static readonly Guid TokenId = Guid.NewGuid();

    private readonly Mock<IUserRefreshTokenRepository> _refreshTokens = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private readonly UserRefreshToken _stored;
    private readonly LogoutCommandHandler _handler;

    private static string ValidToken => $"{TokenId}.{Secret}";

    public LogOutCommandHandlerTests()
    {
        _stored = new UserRefreshToken(
            TokenId, Guid.NewGuid(), StoredHash, DateTime.UtcNow.AddDays(7), null);

        _refreshTokens.Setup(r => r.GetById(TokenId, It.IsAny<CancellationToken>())).ReturnsAsync(_stored);

        _hasher.Setup(h => h.VerifyPassword(Secret, StoredHash)).Returns(true);

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new LogoutCommandHandler(_refreshTokens.Object, _hasher.Object, _uow.Object);
    }

    private Task LogOut(string token, CancellationToken ct = default) =>
        _handler.Handle(new LogoutCommand(token), ct);

    private void VerifyNothingSaved() =>
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

    // ---------------------------------------------------------------- rejected requests

    [Theory]
    [InlineData("")]
    [InlineData("no-separator")]
    [InlineData("a.b.c")]
    public async Task InvalidFormat_ThrowsBadRequest_AndRevokesNothing(string token)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => LogOut(token));

        _refreshTokens.Verify(r => r.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Null(_stored.RefreshTokenRevokedAt);
        VerifyNothingSaved();
    }

    [Fact]
    public async Task UnknownToken_ThrowsNotFound_AndRevokesNothing()
    {
        var otherId = Guid.NewGuid();
        _refreshTokens.Setup(r => r.GetById(otherId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((UserRefreshToken?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => LogOut($"{otherId}.{Secret}"));

        Assert.Null(_stored.RefreshTokenRevokedAt);
        VerifyNothingSaved();
    }

    [Fact]
    public async Task WrongSecret_ThrowsUnauthorized_AndRevokesNothing()
    {
        _hasher.Setup(h => h.VerifyPassword("wrong", StoredHash)).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedException>(() => LogOut($"{TokenId}.wrong"));

        Assert.Null(_stored.RefreshTokenRevokedAt);
        VerifyNothingSaved();
    }

    // ---------------------------------------------------------------- successful logout

    [Fact]
    public async Task ValidToken_RevokesIt()
    {
        await LogOut(ValidToken);

        Assert.NotNull(_stored.RefreshTokenRevokedAt);
    }

    [Fact]
    public async Task ValidToken_SavesChangesOnce()
    {
        await LogOut(ValidToken);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidToken_VerifiesSecretAgainstStoredHash()
    {
        await LogOut(ValidToken);

        _hasher.Verify(h => h.VerifyPassword(Secret, StoredHash), Times.Once);
    }

    // ---------------------------------------------------------------- cancellation

    [Fact]
    public async Task CancellationToken_IsPassedToSaveChanges()
    {
        using var cts = new CancellationTokenSource();

        await LogOut(ValidToken, cts.Token);

        _uow.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }
}






