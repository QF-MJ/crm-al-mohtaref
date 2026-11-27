using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Auth.ResetPassword;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Application.Interfaces.Common;
using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Contacts;
using CRM.AlMohtaref.Domain.Entities.Users;
using Moq;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Auth.ResetPassword;

public class ResetPasswordCommandHandlerTests
{
    private const string OldHash = "old-hash";
    private const string NewPassword = "NewPassw0rd!";
    private const string NewHash = "new-hash";

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUserRefreshTokenRepository> _refreshTokens = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private readonly List<string> _calls = new();

    private readonly User _user;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _user = new User(Guid.NewGuid(), "acc", OldHash, new Person("Full Name", Guid.NewGuid()), "a@b.com");

        _users.Setup(u => u.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_user);

        _hasher.Setup(h => h.HashPassword(NewPassword)).Returns(NewHash);

        _refreshTokens.Setup(r => r.RevokeUserTokens(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                      .Callback<Guid, CancellationToken>((_, _) => _calls.Add("revoke"))
                      .Returns(Task.CompletedTask);

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("save"))
            .ReturnsAsync(1);

        _handler = new ResetPasswordCommandHandler(
            _users.Object, _refreshTokens.Object, _hasher.Object, _uow.Object);
    }

    private Task<bool> Reset(
        Guid? userId = null,
        string newPassword = NewPassword,
        string? confirmPassword = null,
        CancellationToken ct = default) =>
        _handler.Handle(
            new ResetPasswordCommand(userId ?? _user.Id, newPassword, confirmPassword ?? newPassword), ct);

    private void VerifyNothingChanged()
    {
        _hasher.Verify(h => h.HashPassword(It.IsAny<string>()), Times.Never);
        _refreshTokens.Verify(
            r => r.RevokeUserTokens(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private IEnumerable<string?> StringValuesOfUser() =>
        typeof(User)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(string) && p.CanRead && p.GetIndexParameters().Length == 0)
            .Select(p => (string?)p.GetValue(_user));

    // ---------------------------------------------------------------- rejected requests

    [Fact]
    public async Task UnknownUser_ThrowsNotFound_AndChangesNothing()
    {
        var otherId = Guid.NewGuid();
        _users.Setup(u => u.GetByIdAsync(otherId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => Reset(userId: otherId));

        Assert.Equal(404, ex.StatusCode);
        VerifyNothingChanged();
    }

    [Fact]
    public async Task PasswordsDoNotMatch_ThrowsBadRequest_AndChangesNothing()
    {
        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => Reset(confirmPassword: "something-else"));

        Assert.Equal(400, ex.StatusCode);
        VerifyNothingChanged();
        Assert.Contains(OldHash, StringValuesOfUser());
    }

    [Fact]
    public async Task PasswordsDifferOnlyByCase_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(
            () => Reset(newPassword: NewPassword, confirmPassword: NewPassword.ToLowerInvariant()));

        VerifyNothingChanged();
    }

    // ---------------------------------------------------------------- successful reset

    [Fact]
    public async Task ValidRequest_ReturnsTrue()
    {
        var result = await Reset();

        Assert.True(result);
    }

    [Fact]
    public async Task ValidRequest_HashesTheNewPasswordOnce()
    {
        await Reset();

        _hasher.Verify(h => h.HashPassword(NewPassword), Times.Once);
        _hasher.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ValidRequest_StoresTheHash_NotThePlainPassword()
    {
        await Reset();

        var values = StringValuesOfUser().ToList();

        Assert.Contains(NewHash, values);
        Assert.DoesNotContain(NewPassword, values);
        Assert.DoesNotContain(OldHash, values);
    }

    [Fact]
    public async Task ValidRequest_RevokesAllRefreshTokensOfThatUser()
    {
        await Reset();

        _refreshTokens.Verify(
            r => r.RevokeUserTokens(_user.Id, It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokens.Verify(
            r => r.RevokeUserTokens(It.Is<Guid>(id => id != _user.Id), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidRequest_SavesExactlyOnce()
    {
        await Reset();

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidRequest_SaveRunsAfterRevoke()
    {
        await Reset();

        Assert.Equal(new[] { "revoke", "save" }, _calls);
    }

    [Fact]
    public async Task NothingSaved_ReturnsFalse()
    {
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await Reset();

        Assert.False(result);
    }

    // ---------------------------------------------------------------- cancellation

    [Fact]
    public async Task CancellationToken_IsPassedToUserRepository()
    {
        using var cts = new CancellationTokenSource();

        await Reset(ct: cts.Token);

        _users.Verify(u => u.GetByIdAsync(_user.Id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task CancellationToken_IsPassedToSaveChanges()
    {
        using var cts = new CancellationTokenSource();

        await Reset(ct: cts.Token);

        _uow.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }
}
