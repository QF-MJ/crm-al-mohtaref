using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Auth.Logout;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Application.Interfaces.Common;
using CRM.AlMohtaref.Domain.Entities.Users;
using Moq;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Auth.Logout;

/// <summary>
/// Tests tagged Category=BugProbe are expected to FAIL on the current code.
/// </summary>
public class LogoutCommandHandlerTests
{
    private const string Secret = "secret";
    private const string StoredHash = "stored-hash";

    private readonly Mock<IUserRefreshTokenRepository> _refreshTokens = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    public LogoutCommandHandlerTests()
    {
        _hasher.Setup(h => h.VerifyPassword(Secret, StoredHash)).Returns(true);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private LogoutCommandHandler Sut => new(_refreshTokens.Object, _hasher.Object, _uow.Object);

    // NOTE: if IUserRefreshTokenRepository.GetById has a CancellationToken parameter, change it here only.
    private UserRefreshToken StoredToken(DateTime? revokedAt = null)
    {
        var stored = new UserRefreshToken(Guid.NewGuid(), Guid.NewGuid(), StoredHash,
            DateTime.UtcNow.AddDays(1), revokedAt);
        _refreshTokens.Setup(r => r.GetById(stored.Id, CancellationToken.None)).ReturnsAsync(stored);
        return stored;
    }

    private Task Logout(string raw, CancellationToken ct = default) =>
        Sut.Handle(new LogoutCommand(raw), ct);

    [Fact]
    public async Task ValidToken_IsRevoked_AndSaved()
    {
        var stored = StoredToken();
        var before = DateTime.UtcNow;

        await Logout($"{stored.Id}.{Secret}");

        Assert.NotNull(stored.RefreshTokenRevokedAt);
        Assert.InRange(stored.RefreshTokenRevokedAt!.Value, before, DateTime.UtcNow);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Save_ReceivesTheCancellationToken()
    {
        var stored = StoredToken();
        using var cts = new CancellationTokenSource();

        await Logout($"{stored.Id}.{Secret}", cts.Token);

        _uow.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once());
    }

    [Fact]
    public async Task UnknownToken_ThrowsNotFound_AndDoesNotSave()
    {
        var id = Guid.NewGuid();
        _refreshTokens.Setup(r => r.GetById(id, CancellationToken.None)).ReturnsAsync((UserRefreshToken?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => Logout($"{id}.{Secret}"));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task WrongSecret_ThrowsUnauthorized_AndDoesNotRevoke()
    {
        var stored = StoredToken();

        await Assert.ThrowsAsync<UnauthorizedException>(() => Logout($"{stored.Id}.wrong"));

        Assert.Null(stored.RefreshTokenRevokedAt);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    // ------------------------------------------------------------ BUG PROBES (expected to FAIL today)

    [Theory]
    [Trait("Category", "BugProbe")]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("a.b.c")]
    [InlineData("not-a-guid.secret")]
    [InlineData(".")]
    public async Task MalformedToken_ShouldBeBadRequest(string raw)
    {
        // Today: FormatException / IndexOutOfRangeException -> HTTP 500.
        await Assert.ThrowsAsync<BadRequestException>(() => Logout(raw));
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public async Task NullToken_ShouldBeBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => Logout(null!));
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public async Task AlreadyRevokedToken_ShouldKeepItsOriginalRevocationDate()
    {
        var original = DateTime.UtcNow.AddDays(-2);
        var stored = StoredToken(revokedAt: original);

        await Record.ExceptionAsync(() => Logout($"{stored.Id}.{Secret}"));

        Assert.Equal(original, stored.RefreshTokenRevokedAt);
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public async Task RepositoryFailure_ShouldPropagateOriginalException()
    {
        // The handler uses GetById(..).Result (sync-over-async): the real exception gets wrapped
        // in AggregateException and the call can deadlock under a sync context.
        var id = Guid.NewGuid();
        _refreshTokens.Setup(r => r.GetById(id, CancellationToken.None)).ThrowsAsync(new InvalidOperationException("db down"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Logout($"{id}.{Secret}"));
    }
}
