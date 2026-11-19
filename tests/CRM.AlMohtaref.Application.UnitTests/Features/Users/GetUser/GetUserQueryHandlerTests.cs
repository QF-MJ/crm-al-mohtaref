using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Application.Interfaces;
using Moq;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Users;

public class GetUserQueryHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ILookupRepository> _lookup = new();

    private GetUserQueryHandler Sut => new(_users.Object, _currentUser.Object, _lookup.Object);

    private UserDetailDto ArrangeUser(Guid? id = null)
    {
        var dto = new UserDetailDto
        {
            Id = id ?? Guid.NewGuid(),
            Permissions = new List<PermissionDto> { new() }
        };
        _users.Setup(x => x.GetDetailAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        return dto;
    }

    private void CallerIsAdmin()
        => _currentUser.Setup(x => x.HasDepartment(UsersTestData.AdminDepartment)).Returns(true);

    [Fact]
    public async Task UserNotFound_ThrowsNotFound_AndSkipsOwnershipChecks()
    {
        var id = Guid.NewGuid();
        _users.Setup(x => x.GetDetailAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((UserDetailDto?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => Sut.Handle(new GetUserQuery(id), CancellationToken.None));

        Assert.Contains(id.ToString(), ex.Message);
        _currentUser.Verify(x => x.HasDepartment(It.IsAny<string>()), Times.Never());
    }

    [Fact]
    public async Task Admin_CanViewAnyUser()
    {
        var dto = ArrangeUser();
        CallerIsAdmin();

        var result = await Sut.Handle(new GetUserQuery(dto.Id), CancellationToken.None);

        Assert.Same(dto, result);
    }

    [Fact]
    public async Task User_CanViewHimself_WithoutDepartmentCheck()
    {
        var dto = ArrangeUser();
        _currentUser.SetupGet(x => x.UserId).Returns(dto.Id);

        var result = await Sut.Handle(new GetUserQuery(dto.Id), CancellationToken.None);

        Assert.Same(dto, result);
        _currentUser.Verify(x => x.HasAnyDepartment(dto.Permissions), Times.Never());
    }

    [Fact]
    public async Task SameDepartment_CanView()
    {
        var dto = ArrangeUser();
        _currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        _currentUser.Setup(x => x.HasAnyDepartment(dto.Permissions)).Returns(true);

        var result = await Sut.Handle(new GetUserQuery(dto.Id), CancellationToken.None);

        Assert.Same(dto, result);
    }

    [Fact]
    public async Task OtherDepartment_ThrowsForbidden_WithUserIdInMessage()
    {
        var dto = ArrangeUser();
        _currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        _currentUser.Setup(x => x.HasAnyDepartment(dto.Permissions)).Returns(false);

        var ex = await Assert.ThrowsAsync<ForbiddenException>(
            () => Sut.Handle(new GetUserQuery(dto.Id), CancellationToken.None));

        Assert.Contains(dto.Id.ToString(), ex.Message);
    }

    [Fact]
    public async Task TargetWithNoPermissions_AndCallerNotAdmin_IsForbidden()
    {
        var dto = ArrangeUser();
        dto.Permissions = new List<PermissionDto>();
        _currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());

        await Assert.ThrowsAsync<ForbiddenException>(
            () => Sut.Handle(new GetUserQuery(dto.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ForwardsCancellationTokenToRepository()
    {
        var dto = ArrangeUser();
        CallerIsAdmin();
        using var cts = new CancellationTokenSource();

        await Sut.Handle(new GetUserQuery(dto.Id), cts.Token);

        _users.Verify(x => x.GetDetailAsync(dto.Id, It.Is<CancellationToken>(t => t == cts.Token)), Times.Once());
    }

    [Fact]
    public async Task RepositoryFailure_Propagates()
    {
        _users.Setup(x => x.GetDetailAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
              .ThrowsAsync(new InvalidOperationException("db"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Sut.Handle(new GetUserQuery(Guid.NewGuid()), CancellationToken.None));
    }

    // ---- IsOwnerAsync directly ----

    [Fact]
    public async Task IsOwner_Admin_True()
    {
        CallerIsAdmin();
        Assert.True(await Sut.IsOwnerAsync(new UserDetailDto { Id = Guid.NewGuid() }, CancellationToken.None));
    }

    [Fact]
    public async Task IsOwner_Stranger_False()
    {
        _currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        Assert.False(await Sut.IsOwnerAsync(new UserDetailDto { Id = Guid.NewGuid() }, CancellationToken.None));
    }
}