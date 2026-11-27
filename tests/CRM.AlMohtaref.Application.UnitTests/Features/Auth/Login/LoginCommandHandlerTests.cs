using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Auth;
using CRM.AlMohtaref.Application.Features.Auth.Login;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Application.Interfaces.Common;
using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Contacts;
using CRM.AlMohtaref.Domain.Entities.Users;
using Moq;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Auth.Login;


public class LoginCommandHandlerTests
{
    private const string Account = "acc";
    private const string StoredHash = "stored-hash";
    private const string RawRefreshToken = "raw-refresh";

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUserRefreshTokenRepository> _refreshTokens = new();

    private readonly List<string> _calls = new();
    private UserRefreshToken? _addedRefreshToken;

    private readonly User _user;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _user = new User(Guid.NewGuid(), Account, StoredHash, new Person("Full Name", Guid.NewGuid()), "a@b.com");

        _users.Setup(u => u.GetByAccountNameAsync(Account, It.IsAny<CancellationToken>())).ReturnsAsync(_user);
        _hasher.Setup(h => h.VerifyPassword("good", StoredHash)).Returns(true);
        _hasher.Setup(h => h.VerifyPassword("bad", StoredHash)).Returns(false);

        _hasher.Setup(h => h.HashPassword(RawRefreshToken)).Returns("hashed-refresh-token");
        _tokens.Setup(t => t.GenerateAccessToken(_user)).ReturnsAsync("jwt");
        _tokens.Setup(t => t.GenerateRefreshToken()).Returns(RawRefreshToken);
        _tokens.Setup(t => t.GetExpireAt()).Returns(new DateTime(2030, 1, 1));




        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("save"))
            .ReturnsAsync(1);

        _refreshTokens.Setup(r => r.Add(It.IsAny<UserRefreshToken>()))
            .Callback<UserRefreshToken>(t =>
            {
                _calls.Add("add");
                _addedRefreshToken = t;
            });

        _handler = new LoginCommandHandler(
            _users.Object, _hasher.Object, _tokens.Object, _uow.Object, _refreshTokens.Object);
    }

    private Task<LoginDto> Login(string password, CancellationToken ct = default) =>
        _handler.Handle(new LoginCommand(Account, password), ct);

    private async Task<string> MessageOfFailedLogin(LoginCommand command)
    {
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _handler.Handle(command, CancellationToken.None));
        return ex.Message;
    }

    // ---------------------------------------------------------------- rejected logins

    [Fact]
    public async Task UnknownAccount_ThrowsUnauthorized_AndIssuesNothing()
    {
        _users.Setup(u => u.GetByAccountNameAsync("nobody", It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _handler.Handle(new LoginCommand("nobody", "good"), CancellationToken.None));

        _tokens.Verify(t => t.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        _refreshTokens.Verify(r => r.Add(It.IsAny<UserRefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task InactiveUser_ThrowsUnauthorized_EvenWithCorrectPassword()
    {
        _user.UpdateProfile(Account, "a@b.com", false, "Full Name");

        await Assert.ThrowsAsync<UnauthorizedException>(() => Login("good"));

        _tokens.Verify(t => t.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        _refreshTokens.Verify(r => r.Add(It.IsAny<UserRefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task WrongPassword_ThrowsUnauthorized_AndIssuesNothing()
    {
        await Assert.ThrowsAsync<UnauthorizedException>(() => Login("bad"));

        _tokens.Verify(t => t.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        _tokens.Verify(t => t.GenerateRefreshToken(), Times.Never);
        _refreshTokens.Verify(r => r.Add(It.IsAny<UserRefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task WrongPassword_CountsFailure_AndSavesOnce()
    {
        await Assert.ThrowsAsync<UnauthorizedException>(() => Login("bad"));

        Assert.Equal(1, _user.FailedLoginCount);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnknownAccount_AndWrongPassword_ReturnIdenticalMessages()
    {
        _users.Setup(u => u.GetByAccountNameAsync("nobody", It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);

        var unknownAccount = await MessageOfFailedLogin(new LoginCommand("nobody", "good"));
        var wrongPassword = await MessageOfFailedLogin(new LoginCommand(Account, "bad"));

        Assert.Equal(unknownAccount, wrongPassword);
    }

    // ---------------------------------------------------------------- lockout

    [Fact]
    public async Task FifthWrongPassword_LocksAccount()
    {
        for (var i = 0; i < 5; i++)
            await Assert.ThrowsAsync<UnauthorizedException>(() => Login("bad"));

        Assert.NotNull(_user.LockoutEnd);
        Assert.True(_user.LockoutEnd > DateTime.UtcNow);
    }

    [Fact]
    public async Task FourthWrongPassword_DoesNotLockYet()
    {
        for (var i = 0; i < 4; i++)
            await Assert.ThrowsAsync<UnauthorizedException>(() => Login("bad"));

        Assert.Null(_user.LockoutEnd);
    }

    [Fact]
    public async Task LockedAccount_ThrowsForbidden_EvenWithCorrectPassword()
    {
        _user.LockAccount(DateTime.UtcNow.AddDays(1));

        await Assert.ThrowsAsync<ForbiddenException>(() => Login("good"));

        _tokens.Verify(t => t.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        _refreshTokens.Verify(r => r.Add(It.IsAny<UserRefreshToken>()), Times.Never);
    }


    [Fact]
    public async Task AccountLockedForThirtyMoreMinutesUtc_IsStillLocked()
    {
        _user.LockAccount(DateTime.UtcNow.AddMinutes(30));

        await Assert.ThrowsAsync<ForbiddenException>(() => Login("good"));
    }


    [Fact]
    public async Task WrongPasswordAfterLockExpired_LocksAccountAgain()
    {
        for (var i = 0; i < 5; i++)
            await Assert.ThrowsAsync<UnauthorizedException>(() => Login("bad"));

        _user.LockAccount(DateTime.UtcNow.AddMinutes(-1)); // simulate the lock expiring

        await Assert.ThrowsAsync<UnauthorizedException>(() => Login("bad"));

        Assert.True(_user.LockoutEnd > DateTime.UtcNow);
    }

    // ---------------------------------------------------------------- successful login

    [Fact]
    public async Task CorrectPassword_ReturnsAccessTokenAndUserInfo()
    {
        var result = await Login("good");

        Assert.Equal("jwt", result.Token);
        Assert.Equal(_user.Id, result.UserId);
        Assert.Equal(new DateTime(2030, 1, 1), result.ExpiresIn);
        Assert.Empty(result.Permissions);
        _tokens.Verify(t => t.GenerateAccessToken(_user), Times.Once);
    }

    [Fact]
    public async Task CorrectPassword_ResetsFailedLoginCount()
    {
        _user.RegisterFailedLogin();
        _user.RegisterFailedLogin();

        await Login("good");

        Assert.Equal(0, _user.FailedLoginCount);
    }

    // ---------------------------------------------------------------- refresh token

    [Fact]
    public async Task CorrectPassword_ReturnsRefreshToken_AsIdDotSecret()
    {
        var result = await Login("good");

        Assert.NotNull(result.RefreshToken);
        var separator = result.RefreshToken!.IndexOf('.');
        Assert.True(separator > 0, "refresh token must look like '<guid>.<secret>'");
        Assert.True(Guid.TryParse(result.RefreshToken[..separator], out var id));
        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal(RawRefreshToken, result.RefreshToken[(separator + 1)..]);
    }

    [Fact]
    public async Task CorrectPassword_StoresExactlyOneRefreshToken()
    {
        await Login("good");

        _refreshTokens.Verify(r => r.Add(It.IsAny<UserRefreshToken>()), Times.Once);
        Assert.NotNull(_addedRefreshToken);
    }

    [Fact]
    public async Task StoredRefreshToken_DoesNotContainThePlainSecret()
    {
        var result = await Login("good");

        var stringValues = typeof(UserRefreshToken)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(string) && p.CanRead)
            .Select(p => (string?)p.GetValue(_addedRefreshToken));

        Assert.All(stringValues, v =>
        {
            Assert.NotEqual(RawRefreshToken, v);
            Assert.NotEqual(result.RefreshToken, v);
        });
    }

    [Fact]
    public async Task RefreshToken_IsPersisted_SaveRunsAfterAdd()
    {
        await Login("good");

        var addIndex = _calls.IndexOf("add");
        Assert.True(addIndex >= 0, "refresh token was never added");
        Assert.Contains("save", _calls.Skip(addIndex + 1));
    }

    // ---------------------------------------------------------------- cancellation

    [Fact]
    public async Task CancellationToken_IsPassedToUserRepository()
    {
        using var cts = new CancellationTokenSource();

        await Login("good", cts.Token);

        _users.Verify(u => u.GetByAccountNameAsync(Account, cts.Token), Times.Once);
    }


    [Fact]
    public async Task CancellationToken_IsPassedToSaveChanges()
    {
        using var cts = new CancellationTokenSource();

        await Login("good", cts.Token);

        _uow.Verify(u => u.SaveChangesAsync(cts.Token), Times.AtLeastOnce);
    }
}