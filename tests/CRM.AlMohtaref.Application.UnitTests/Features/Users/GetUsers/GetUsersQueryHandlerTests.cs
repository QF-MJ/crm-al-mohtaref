using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Lookups.Dtos;
using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Application.Interfaces.Common;
using CRM.AlMohtaref.Domain.Entities.Users;
using Moq;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Users;

public class GetUsersQueryHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ILookupRepository> _lookup = new();

    private GetUsersQueryHandler Sut => new(_users.Object, _currentUser.Object, _lookup.Object);

    private void CallerIsAdmin()
        => _currentUser.Setup(x => x.HasDepartment(UsersTestData.AdminDepartment)).Returns(true);


    private void ArrangeDepartment(Guid id, string? name)
        => _lookup.Setup(x => x.GetDepartmentByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(name is null ? null : new LookupItemDto(id, name));

    private UsersPageDto ArrangeSearch(GetUsersQuery query)
    {
        var page = new UsersPageDto();
        _users.Setup(x => x.SearchAsync(query, It.IsAny<CancellationToken>())).ReturnsAsync(page);
        return page;
    }

    [Fact]
    public async Task Admin_WithoutDepartmentFilter_Searches_AndNeverQueriesLookup()
    {
        CallerIsAdmin();
        var query = new GetUsersQuery();
        var page = ArrangeSearch(query);

        var result = await Sut.Handle(query, CancellationToken.None);

        Assert.Same(page, result);
        _lookup.Verify(x => x.GetDepartmentByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Admin_WithAnyDepartmentFilter_Searches()
    {
        CallerIsAdmin();
        var query = new GetUsersQuery(DepartmentId: Guid.NewGuid());
        var page = ArrangeSearch(query);

        Assert.Same(page, await Sut.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task NonAdmin_WithoutDepartmentFilter_IsForbidden_AndNeverSearches()
    {
        var query = new GetUsersQuery();

        await Assert.ThrowsAsync<ForbiddenException>(() => Sut.Handle(query, CancellationToken.None));

        _users.Verify(x => x.SearchAsync(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task NonAdmin_DepartmentDoesNotExist_IsForbidden()
    {
        var deptId = Guid.NewGuid();
        ArrangeDepartment(deptId, null);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => Sut.Handle(new GetUsersQuery(DepartmentId: deptId), CancellationToken.None));
    }

    [Fact]
    public async Task NonAdmin_ForeignDepartment_IsForbidden_AndNeverSearches()
    {
        var deptId = Guid.NewGuid();
        ArrangeDepartment(deptId, UsersTestData.SalesDepartment);
        _currentUser.Setup(x => x.HasDepartment(UsersTestData.SalesDepartment)).Returns(false);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => Sut.Handle(new GetUsersQuery(DepartmentId: deptId), CancellationToken.None));

        _users.Verify(x => x.SearchAsync(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task NonAdmin_OwnDepartment_Searches()
    {
        var deptId = Guid.NewGuid();
        ArrangeDepartment(deptId, UsersTestData.SalesDepartment);
        _currentUser.Setup(x => x.HasDepartment(UsersTestData.SalesDepartment)).Returns(true);
        var query = new GetUsersQuery(DepartmentId: deptId);
        var page = ArrangeSearch(query);

        Assert.Same(page, await Sut.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task NonAdmin_EmptyGuidDepartment_IsForbidden()
    {
        ArrangeDepartment(Guid.Empty, null);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => Sut.Handle(new GetUsersQuery(DepartmentId: Guid.Empty), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ForwardsCancellationTokenToSearch()
    {
        CallerIsAdmin();
        var query = new GetUsersQuery();
        ArrangeSearch(query);
        using var cts = new CancellationTokenSource();

        await Sut.Handle(query, cts.Token);

        _users.Verify(x => x.SearchAsync(query, It.Is<CancellationToken>(t => t == cts.Token)), Times.Once());
    }

    [Fact]
    public async Task LookupFailure_Propagates()
    {
        _lookup.Setup(x => x.GetDepartmentByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidOperationException("db"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Sut.Handle(new GetUsersQuery(DepartmentId: Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task SearchFailure_Propagates()
    {
        CallerIsAdmin();
        _users.Setup(x => x.SearchAsync(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
              .ThrowsAsync(new InvalidOperationException("db"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.Handle(new GetUsersQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task ForbiddenMessage_IsTheDepartmentMessage()
    {
        var ex = await Assert.ThrowsAsync<ForbiddenException>(
            () => Sut.Handle(new GetUsersQuery(), CancellationToken.None));

        Assert.Equal("البيانات المطلوبة لا تنتمي إلى القسم الخاص بك", ex.Message);
    }
}