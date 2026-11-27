using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Application.Interfaces.Common;
using CRM.AlMohtaref.Domain.Entities.Users;
using Moq;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Users;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUserRefreshTokenRepository> _tokens = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private UpdateUserCommandHandler Sut => new(_users.Object, _tokens.Object, _unitOfWork.Object);

    private User ArrangeExistingUser(params (Role Role, Guid DepartmentId)[] permissions)
    {
        var user = UsersTestData.NewUser();
        user.SynchronizePermissions(permissions);
        _users.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        return user;
    }

    private static UpdateUserCommand CommandFor(User user, bool? isActive, params (Role Role, Guid DepartmentId)[] permissions)
        => new(user.Id, "new.account", "New Name", "new@example.com", isActive, UsersTestData.Perms(permissions));

    private void VerifyTokensRevoked(Guid userId, Times times)
        => _tokens.Verify(x => x.RevokeUserTokens(userId, It.IsAny<CancellationToken>()), times);

    // ------------------------------ guard clauses ------------------------------

    [Fact]
    public async Task UserNotFound_ThrowsNotFound_AndTouchesNothingElse()
    {
        var id = Guid.NewGuid();
        _users.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => Sut.Handle(UsersTestData.ValidUpdate(id), CancellationToken.None));

        Assert.Contains(id.ToString(), ex.Message);
        _users.Verify(x => x.ExistsAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task DuplicateEmailOrAccount_ThrowsConflict_AndLeavesUserUntouched()
    {
        var user = ArrangeExistingUser((Role.Employee, Guid.NewGuid()));
        var (oldAccount, oldEmail, oldName) = (user.AccountName, user.Email, user.FullName);
        _users.Setup(x => x.ExistsAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(
            () => Sut.Handle(CommandFor(user, true, (Role.Manager, Guid.NewGuid())), CancellationToken.None));

        Assert.Equal(oldAccount, user.AccountName);
        Assert.Equal(oldEmail, user.Email);
        Assert.Equal(oldName, user.FullName);
        VerifyTokensRevoked(user.Id, Times.Never());
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task UniquenessCheck_ExcludesTheUserBeingUpdated()
    {
        var user = ArrangeExistingUser((Role.Employee, Guid.NewGuid()));
        var cmd = CommandFor(user, true, (Role.Employee, Guid.NewGuid()));

        await Sut.Handle(cmd, CancellationToken.None);

        _users.Verify(x => x.ExistsAsync(user.Id, cmd.Email, cmd.AccountName, It.IsAny<CancellationToken>()), Times.Once());
    }

    // ------------------------------ profile ------------------------------

    [Fact]
    public async Task HappyPath_UpdatesProfile_AndSavesOnce()
    {
        var user = ArrangeExistingUser((Role.Employee, Guid.NewGuid()));
        var cmd = CommandFor(user, true, (Role.Employee, user.UserPermissions.First().DepartmentId));

        await Sut.Handle(cmd, CancellationToken.None);

        Assert.Equal("new.account", user.AccountName);
        Assert.Equal("new@example.com", user.Email);
        Assert.Equal("New Name", user.FullName);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Theory]
    [InlineData(true, null, true)]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, null, false)]
    public async Task IsActive_FollowsRequest_OrKeepsCurrentWhenNull(bool initial, bool? requested, bool expected)
    {
        var user = ArrangeExistingUser((Role.Employee, Guid.NewGuid()));
        if (!initial) user.UpdateProfile(user.AccountName, user.Email, false, user.FullName);
        var perms = user.UserPermissions.Select(p => (p.Role, p.DepartmentId)).ToArray();

        await Sut.Handle(CommandFor(user, requested, perms), CancellationToken.None);

        Assert.Equal(expected, user.IsActive);
    }

    // ------------------------------ permissions & token revocation ------------------------------

    [Fact]
    public async Task PermissionsUnchanged_DoNotRevokeTokens_NorRecreateEntities()
    {
        var dept = Guid.NewGuid();
        var user = ArrangeExistingUser((Role.Employee, dept));
        var original = user.UserPermissions.Single();

        await Sut.Handle(CommandFor(user, true, (Role.Employee, dept)), CancellationToken.None);

        VerifyTokensRevoked(user.Id, Times.Never());
        Assert.Same(original, Assert.Single(user.UserPermissions));
    }

    [Fact]
    public async Task SameSetInDifferentOrder_IsNotAChange()
    {
        var (d1, d2) = (Guid.NewGuid(), Guid.NewGuid());
        var user = ArrangeExistingUser((Role.Employee, d1), (Role.Manager, d2));

        await Sut.Handle(CommandFor(user, true, (Role.Manager, d2), (Role.Employee, d1)), CancellationToken.None);

        VerifyTokensRevoked(user.Id, Times.Never());
        Assert.Equal(2, user.UserPermissions.Count);
    }

    [Fact]
    public async Task DuplicatesInRequest_DoNotCreateChangeOrDuplicates()
    {
        var dept = Guid.NewGuid();
        var user = ArrangeExistingUser((Role.Employee, dept));

        await Sut.Handle(CommandFor(user, true, (Role.Employee, dept), (Role.Employee, dept)), CancellationToken.None);

        VerifyTokensRevoked(user.Id, Times.Never());
        Assert.Single(user.UserPermissions);
    }

    [Fact]
    public async Task PermissionAdded_SyncsAndRevokesTokens()
    {
        var (d1, d2) = (Guid.NewGuid(), Guid.NewGuid());
        var user = ArrangeExistingUser((Role.Employee, d1));

        await Sut.Handle(CommandFor(user, true, (Role.Employee, d1), (Role.Manager, d2)), CancellationToken.None);

        Assert.Equal(2, user.UserPermissions.Count);
        Assert.Contains(user.UserPermissions, p => p.DepartmentId == d2 && p.Role == Role.Manager);
        VerifyTokensRevoked(user.Id, Times.Once());
    }

    [Fact]
    public async Task PermissionRemoved_SyncsAndRevokesTokens()
    {
        var (d1, d2) = (Guid.NewGuid(), Guid.NewGuid());
        var user = ArrangeExistingUser((Role.Employee, d1), (Role.Manager, d2));

        await Sut.Handle(CommandFor(user, true, (Role.Employee, d1)), CancellationToken.None);

        var remaining = Assert.Single(user.UserPermissions);
        Assert.Equal(d1, remaining.DepartmentId);
        VerifyTokensRevoked(user.Id, Times.Once());
    }

    [Fact]
    public async Task RoleChangedInSameDepartment_RevokesTokens()
    {
        var dept = Guid.NewGuid();
        var user = ArrangeExistingUser((Role.Employee, dept));

        await Sut.Handle(CommandFor(user, true, (Role.Manager, dept)), CancellationToken.None);

        var permission = Assert.Single(user.UserPermissions);
        Assert.Equal(Role.Manager, permission.Role);
        VerifyTokensRevoked(user.Id, Times.Once());
    }

    [Fact]
    public async Task EmptyPermissionList_RemovesEverything_AndRevokesTokens()
    {
        var user = ArrangeExistingUser((Role.Employee, Guid.NewGuid()), (Role.Manager, Guid.NewGuid()));

        await Sut.Handle(CommandFor(user, true), CancellationToken.None);

        Assert.Empty(user.UserPermissions);
        VerifyTokensRevoked(user.Id, Times.Once());
    }

    [Fact]
    public async Task Deactivation_RevokesTokens_EvenWhenPermissionsAreTheSame()
    {
        var dept = Guid.NewGuid();
        var user = ArrangeExistingUser((Role.Employee, dept));

        await Sut.Handle(CommandFor(user, false, (Role.Employee, dept)), CancellationToken.None);

        Assert.False(user.IsActive);
        VerifyTokensRevoked(user.Id, Times.Once());
    }

    [Fact]
    public async Task Revocation_HappensAtMostOnce_WhenBothTriggersApply()
    {
        var user = ArrangeExistingUser((Role.Employee, Guid.NewGuid()));

        await Sut.Handle(CommandFor(user, false, (Role.Manager, Guid.NewGuid())), CancellationToken.None);

        VerifyTokensRevoked(user.Id, Times.Once());
    }

    // ------------------------------ plumbing ------------------------------

    [Fact]
    public async Task CancellationToken_IsForwardedToEveryDependency()
    {
        var user = ArrangeExistingUser((Role.Employee, Guid.NewGuid()));
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        await Sut.Handle(CommandFor(user, false, (Role.Manager, Guid.NewGuid())), token);

        _users.Verify(x => x.GetByIdAsync(user.Id, It.Is<CancellationToken>(t => t == token)), Times.Once());
        _users.Verify(x => x.ExistsAsync(user.Id, It.IsAny<string>(), It.IsAny<string>(), It.Is<CancellationToken>(t => t == token)), Times.Once());
        _tokens.Verify(x => x.RevokeUserTokens(user.Id, It.Is<CancellationToken>(t => t == token)), Times.Once());
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.Is<CancellationToken>(t => t == token)), Times.Once());
    }

    [Fact]
    public async Task SaveFailure_Propagates()
    {
        var user = ArrangeExistingUser((Role.Employee, Guid.NewGuid()));
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("db"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Sut.Handle(CommandFor(user, true, (Role.Employee, Guid.NewGuid())), CancellationToken.None));
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public async Task NullPermissions_ShouldNotCrashTheHandler()
    {
        // Command.Permissions is nullable, but HasPermissionsChanged(null) -> ToHashSet(null) -> ArgumentNullException.
        var user = ArrangeExistingUser((Role.Employee, Guid.NewGuid()));
        var cmd = CommandFor(user, true) with { Permissions = null };

        var ex = await Record.ExceptionAsync(() => Sut.Handle(cmd, CancellationToken.None));

        Assert.Null(ex);
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public async Task NullPermissions_ShouldNotRemoveExistingPermissions()
    {
        var user = ArrangeExistingUser((Role.Employee, Guid.NewGuid()));
        var cmd = CommandFor(user, true) with { Permissions = null };

        await Record.ExceptionAsync(() => Sut.Handle(cmd, CancellationToken.None));

        Assert.Single(user.UserPermissions);
    }
}