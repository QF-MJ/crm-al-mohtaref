using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Tasks.DTOs;
using CRM.AlMohtaref.Application.Features.Tasks.GetTasks;
using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Application.Interfaces;
using Moq;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Tasks.GetTasks;

// Handler = business rules only (existence + ownership/permission + delegation to repository).
// Input-shape rules (empty id, page number, dates...) belong to the validator and are NOT retested here.
public class GetTasksQueryHandlerTests
{
    private const string GeneralManagement = "الإدارة";

    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly GetTasksQueryHandler _handler;

    private readonly Guid _targetUserId = Guid.NewGuid();
    private readonly Guid _callerId = Guid.NewGuid();

    public GetTasksQueryHandlerTests()
    {
        _handler = new GetTasksQueryHandler(_tasks.Object, _users.Object, _currentUser.Object);
        _currentUser.SetupGet(c => c.UserId).Returns(_callerId);
    }

    // NOTE: adjust this helper if UserDetailDto is a record with a constructor.
    private static UserDetailDto UserDto(Guid id, params string[] departments) => new()
    {
        Id = id,
        Permissions = departments.Select(d => new PermissionDto { DepartmentName = d }).ToList()
    };

    private GetTasksQuery Query() => new(_targetUserId);

    private void GivenTargetUser(UserDetailDto? user, CancellationToken ct = default) =>
        _users.Setup(r => r.GetDetailAsync(_targetUserId, ct)).ReturnsAsync(user);

    private void GivenCallerIsManager(bool isManager, Action<List<PermissionDto>>? capture = null) =>
        _currentUser
            .Setup(c => c.IsManagerInAnyDepartment(It.IsAny<IEnumerable<PermissionDto>>()))
            .Callback<IEnumerable<PermissionDto>>(p => capture?.Invoke(p.ToList()))
            .Returns(isManager);

    private TasksPageDto GivenRepositoryReturns(GetTasksQuery query, CancellationToken ct = default)
    {
        var page = new TasksPageDto(null!, null, new List<TaskDto>());
        _tasks.Setup(r => r.SearchAsync(query, ct)).ReturnsAsync(page);
        return page;
    }

    // ---------- Target user does not exist ----------

    [Fact]
    public async System.Threading.Tasks.Task UserNotFound_ThrowsNotFound()
    {
        GivenTargetUser(null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(Query(), default));

        Assert.Contains(_targetUserId.ToString(), ex.Message);
    }

    [Fact]
    public async System.Threading.Tasks.Task UserNotFound_DoesNotSearchTasks_NorCheckPermissions()
    {
        GivenTargetUser(null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(Query(), default));

        _tasks.Verify(r => r.SearchAsync(It.IsAny<GetTasksQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        _currentUser.Verify(c => c.IsManagerInAnyDepartment(It.IsAny<IEnumerable<PermissionDto>>()), Times.Never);
    }

    // ---------- Caller views his own tasks ----------

    [Fact]
    public async System.Threading.Tasks.Task Owner_ReturnsRepositoryResult()
    {
        var query = Query();
        GivenTargetUser(UserDto(_callerId, "المبيعات"));
        var expected = GivenRepositoryReturns(query);

        var result = await _handler.Handle(query, default);

        Assert.Same(expected, result);
    }

    [Fact]
    public async System.Threading.Tasks.Task Owner_DoesNotNeedManagerCheck()
    {
        var query = Query();
        GivenTargetUser(UserDto(_callerId));
        GivenRepositoryReturns(query);

        await _handler.Handle(query, default);

        _currentUser.Verify(c => c.IsManagerInAnyDepartment(It.IsAny<IEnumerable<PermissionDto>>()), Times.Never);
    }

    // ---------- Caller views someone else's tasks ----------

    [Fact]
    public async System.Threading.Tasks.Task NotOwner_NotManager_ThrowsForbidden()
    {
        GivenTargetUser(UserDto(_targetUserId, "المبيعات"));
        GivenCallerIsManager(false);

        var ex = await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(Query(), default));

        Assert.Contains(_targetUserId.ToString(), ex.Message);
    }

    [Fact]
    public async System.Threading.Tasks.Task NotOwner_NotManager_DoesNotSearchTasks()
    {
        GivenTargetUser(UserDto(_targetUserId, "المبيعات"));
        GivenCallerIsManager(false);

        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(Query(), default));

        _tasks.Verify(r => r.SearchAsync(It.IsAny<GetTasksQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async System.Threading.Tasks.Task NotOwner_Manager_ReturnsRepositoryResult()
    {
        var query = Query();
        GivenTargetUser(UserDto(_targetUserId, "المبيعات"));
        GivenCallerIsManager(true);
        var expected = GivenRepositoryReturns(query);

        var result = await _handler.Handle(query, default);

        Assert.Same(expected, result);
    }

    [Fact]
    public async System.Threading.Tasks.Task NotOwner_ManagerCheck_ReceivesTargetUserDepartmentsPlusGeneralManagement()
    {
        var query = Query();
        List<PermissionDto>? received = null;
        GivenTargetUser(UserDto(_targetUserId, "المبيعات", "التسويق"));
        GivenCallerIsManager(true, p => received = p);
        GivenRepositoryReturns(query);

        await _handler.Handle(query, default);

        Assert.NotNull(received);
        Assert.Equal(3, received!.Count);
        Assert.Contains(received, p => p.DepartmentName == "المبيعات");
        Assert.Contains(received, p => p.DepartmentName == "التسويق");
        Assert.Contains(received, p => p.DepartmentName == GeneralManagement);
    }

    [Fact]
    public async System.Threading.Tasks.Task NotOwner_TargetHasNoPermissions_GeneralManagerStillChecked()
    {
        var query = Query();
        List<PermissionDto>? received = null;
        GivenTargetUser(UserDto(_targetUserId));
        GivenCallerIsManager(true, p => received = p);
        GivenRepositoryReturns(query);

        await _handler.Handle(query, default);

        var single = Assert.Single(received!);
        Assert.Equal(GeneralManagement, single.DepartmentName);
    }

    // ---------- Delegation to repositories ----------

    [Fact]
    public async System.Threading.Tasks.Task PassesSameQueryToRepository()
    {
        var query = Query() with { Title = "متابعة", GroupBySubTask = true };
        GivenTargetUser(UserDto(_callerId));
        GivenRepositoryReturns(query);

        await _handler.Handle(query, default);

        _tasks.Verify(r => r.SearchAsync(
            It.Is<GetTasksQuery>(q => ReferenceEquals(q, query)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task PassesCancellationTokenToBothRepositories()
    {
        using var cts = new CancellationTokenSource();
        var query = Query();
        GivenTargetUser(UserDto(_callerId), cts.Token);
        var expected = GivenRepositoryReturns(query, cts.Token);

        // If a different token were forwarded, the mocks would not match
        // (user -> null => NotFoundException, search -> null result).
        var result = await _handler.Handle(query, cts.Token);

        Assert.Same(expected, result);
    }

    [Fact]
    public async System.Threading.Tasks.Task LoadsUserByAssignedToUserId_NotByCaller()
    {
        var query = Query();
        GivenTargetUser(UserDto(_callerId));
        GivenRepositoryReturns(query);

        await _handler.Handle(query, default);

        _users.Verify(r => r.GetDetailAsync(_targetUserId, It.IsAny<CancellationToken>()), Times.Once);
        _users.Verify(r => r.GetDetailAsync(_callerId, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async System.Threading.Tasks.Task RepositoryException_Propagates()
    {
        var query = Query();
        GivenTargetUser(UserDto(_callerId));
        _tasks.Setup(r => r.SearchAsync(query, It.IsAny<CancellationToken>()))
              .ThrowsAsync(new InvalidOperationException("db"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(query, default));
    }
}
