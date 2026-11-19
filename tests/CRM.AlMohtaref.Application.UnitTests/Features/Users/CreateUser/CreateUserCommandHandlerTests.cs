using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Users;
using Moq;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Users;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Guid _currentUserId = Guid.NewGuid();
    private User? _added;

    public CreateUserCommandHandlerTests()
    {
        _currentUser.SetupGet(x => x.UserId).Returns(_currentUserId);
        _hasher.Setup(x => x.HashPassword(It.IsAny<string>())).Returns<string>(p => "HASHED::" + p);
        _users.Setup(x => x.Add(It.IsAny<User>())).Callback<User>(u => _added = u);
    }

    private CreateUserCommandHandler Sut
        => new(_users.Object, _unitOfWork.Object, _currentUser.Object, _hasher.Object);

    [Fact]
    public async Task ValidRequest_AddsUser_Saves_AndReturnsItsId()
    {
        var cmd = UsersTestData.ValidCreate();

        var id = await Sut.Handle(cmd, CancellationToken.None);

        Assert.NotNull(_added);
        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal(_added!.Id, id);
        Assert.Equal(cmd.AccountName, _added.AccountName);
        Assert.Equal(cmd.Email, _added.Email);
        Assert.Equal(cmd.FullName, _added.FullName);
        Assert.True(_added.IsActive);
        Assert.Equal(_currentUserId, _added.CreatedByUserId);
        _users.Verify(x => x.Add(It.IsAny<User>()), Times.Once());
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Password_IsHashedOnce_AndRawPasswordIsNeverStored()
    {
        var cmd = UsersTestData.ValidCreate();

        await Sut.Handle(cmd, CancellationToken.None);

        _hasher.Verify(x => x.HashPassword(cmd.Password), Times.Once());
        Assert.Equal("HASHED::" + cmd.Password, _added!.PasswordHash);
        Assert.NotEqual(cmd.Password, _added.PasswordHash);
    }

    [Fact]
    public async Task NullPermissions_CreatesUserWithoutPermissions()
    {
        var cmd = UsersTestData.ValidCreate() with { Permissions = null };

        await Sut.Handle(cmd, CancellationToken.None);

        Assert.Empty(_added!.UserPermissions);
    }

    [Fact]
    public async Task EmptyPermissions_CreatesUserWithoutPermissions()
    {
        var cmd = UsersTestData.ValidCreate() with { Permissions = new List<PermissionCommand>() };

        await Sut.Handle(cmd, CancellationToken.None);

        Assert.Empty(_added!.UserPermissions);
    }

    [Fact]
    public async Task DuplicatePermissions_AreStoredOnlyOnce()
    {
        var dept = Guid.NewGuid();
        var cmd = UsersTestData.ValidCreate() with
        {
            Permissions = UsersTestData.Perms((Role.Manager, dept), (Role.Manager, dept), (Role.Manager, dept))
        };

        await Sut.Handle(cmd, CancellationToken.None);

        Assert.Single(_added!.UserPermissions);
    }

    [Fact]
    public async Task SameDepartmentWithDifferentRoles_StoresBoth()
    {
        var dept = Guid.NewGuid();
        var cmd = UsersTestData.ValidCreate() with
        {
            Permissions = UsersTestData.Perms((Role.Manager, dept), (Role.Employee, dept))
        };

        await Sut.Handle(cmd, CancellationToken.None);

        Assert.Equal(2, _added!.UserPermissions.Count);
    }

    [Fact]
    public async Task EveryPermission_BelongsToTheCreatedUser()
    {
        var cmd = UsersTestData.ValidCreate() with
        {
            Permissions = UsersTestData.Perms((Role.Manager, Guid.NewGuid()), (Role.Employee, Guid.NewGuid()))
        };

        await Sut.Handle(cmd, CancellationToken.None);

        Assert.All(_added!.UserPermissions, p => Assert.Equal(_added.Id, p.UserId));
    }

    [Fact]
    public async Task TwoCalls_GenerateDifferentIds()
    {
        var first = await Sut.Handle(UsersTestData.ValidCreate(), CancellationToken.None);
        var second = await Sut.Handle(UsersTestData.ValidCreate(), CancellationToken.None);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public async Task SaveFailure_Propagates()
    {
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("db down"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Sut.Handle(UsersTestData.ValidCreate(), CancellationToken.None));
    }


    [Fact]
    [Trait("Category", "BugProbe")]
    public async Task ShouldForwardCancellationTokenToSaveChanges()
    {
        using var cts = new CancellationTokenSource();

        await Sut.Handle(UsersTestData.ValidCreate(), cts.Token);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.Is<CancellationToken>(t => t == cts.Token)), Times.Once());
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public async Task DuplicateEmailOrAccountName_ShouldThrowConflict_AndNotAddUser()
    {
        _users.Setup(x => x.ExistsAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(
            () => Sut.Handle(UsersTestData.ValidCreate(), CancellationToken.None));

        _users.Verify(x => x.Add(It.IsAny<User>()), Times.Never());
    }
}