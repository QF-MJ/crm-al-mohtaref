using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Tasks.Dtos;
using CRM.AlMohtaref.Application.Features.Tasks.GetTask;
using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Application.Interfaces;
using Moq;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Tasks.GetTask;

// Handler = business rules only (existence + ownership/permission + returning what the repository gave).
// Input-shape rules (empty id) belong to the validator and are NOT retested here.
public class GetTaskQueryHandlerTests
{
    private const string GeneralManagement = "الإدارة";

    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly GetTaskQueryHandler _handler;

    private readonly Guid _taskId = Guid.NewGuid();
    private readonly Guid _assignedId = Guid.NewGuid();
    private readonly Guid _callerId = Guid.NewGuid();

    public GetTaskQueryHandlerTests()
    {
        _handler = new GetTaskQueryHandler(_tasks.Object, _users.Object, _currentUser.Object);
        _currentUser.SetupGet(c => c.UserId).Returns(_callerId);
    }

    // NOTE: adjust this helper if UserDetailDto is a record with a constructor.
    // AssignedToUser is nullable in TaskDetailsDto (a task may not be assigned yet).
    private TaskDetailsDto BuildTask(Guid? assignedToUserId) => new(
        Id: _taskId,
        Title: "عنوان",
        Description: null,
        TypeName: "Normal",
        StatusName: "Pending",
        StartTime: new DateTime(2026, 11, 24),
        EndTime: new DateTime(2026, 11, 25),
        CreatedByUser: new TaskUserDto(Guid.NewGuid(), "منشئ"),
        AssignedToUser: assignedToUserId is { } id ? new TaskUserDto(id, "منفذ") : null);

    private static UserDetailDto UserDto(Guid id, params string[] departments) => new()
    {
        Id = id,
        Permissions = departments.Select(d => new PermissionDto { DepartmentName = d }).ToList()
    };

    private GetTaskQuery Query() => new(_taskId);

    private TaskDetailsDto GivenTask(Guid assignedToUserId, CancellationToken ct = default)
    {
        var dto = BuildTask(assignedToUserId);
        _tasks.Setup(r => r.GetDetailAsync(_taskId, ct)).ReturnsAsync(dto);
        return dto;
    }

    private void GivenNoTask(CancellationToken ct = default) =>
        _tasks.Setup(r => r.GetDetailAsync(_taskId, ct)).ReturnsAsync((TaskDetailsDto?)null!);

    private void GivenAssignedUser(UserDetailDto? user, CancellationToken ct = default) =>
        _users.Setup(r => r.GetDetailAsync(_assignedId, ct)).ReturnsAsync(user);

    private void GivenCallerIsManager(bool isManager, Action<List<PermissionDto>>? capture = null) =>
        _currentUser
            .Setup(c => c.IsManagerInAnyDepartment(It.IsAny<IEnumerable<PermissionDto>>()))
            .Callback<IEnumerable<PermissionDto>>(p => capture?.Invoke(p.ToList()))
            .Returns(isManager);

    private void VerifyNoManagerCheck() =>
        _currentUser.Verify(c => c.IsManagerInAnyDepartment(It.IsAny<IEnumerable<PermissionDto>>()), Times.Never);

    // ---------- Task does not exist ----------

    [Fact]
    public async System.Threading.Tasks.Task TaskNotFound_ThrowsNotFound()
    {
        GivenNoTask();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(Query(), default));

        Assert.Contains(_taskId.ToString(), ex.Message);
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskNotFound_DoesNotLoadUser_NorCheckPermissions()
    {
        GivenNoTask();

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(Query(), default));

        _users.Verify(r => r.GetDetailAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        VerifyNoManagerCheck();
    }

    // ---------- Caller is the assigned user ----------

    [Fact]
    public async System.Threading.Tasks.Task Assigned_ReturnsRepositoryResult()
    {
        var expected = GivenTask(_callerId);

        var result = await _handler.Handle(Query(), default);

        Assert.Same(expected, result);
    }

    [Fact]
    public async System.Threading.Tasks.Task Assigned_DoesNotLoadUser_NorCheckManager()
    {
        GivenTask(_callerId);

        await _handler.Handle(Query(), default);

        _users.Verify(r => r.GetDetailAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        VerifyNoManagerCheck();
    }

    // ---------- Caller is not the assigned user ----------

    [Fact]
    public async System.Threading.Tasks.Task NotAssigned_UserNotFound_ThrowsForbidden()
    {
        GivenTask(_assignedId);
        GivenAssignedUser(null);

        var ex = await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(Query(), default));

        Assert.Contains(_assignedId.ToString(), ex.Message);
    }

    [Fact]
    public async System.Threading.Tasks.Task NotAssigned_UserNotFound_DoesNotCheckManager()
    {
        GivenTask(_assignedId);
        GivenAssignedUser(null);

        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(Query(), default));

        VerifyNoManagerCheck();
    }

    [Fact]
    public async System.Threading.Tasks.Task NotAssigned_NotManager_ThrowsForbidden()
    {
        GivenTask(_assignedId);
        GivenAssignedUser(UserDto(_assignedId, "المبيعات"));
        GivenCallerIsManager(false);

        var ex = await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(Query(), default));

        Assert.Contains(_assignedId.ToString(), ex.Message);
    }

    [Fact]
    public async System.Threading.Tasks.Task NotAssigned_Manager_ReturnsRepositoryResult()
    {
        var expected = GivenTask(_assignedId);
        GivenAssignedUser(UserDto(_assignedId, "المبيعات"));
        GivenCallerIsManager(true);

        var result = await _handler.Handle(Query(), default);

        Assert.Same(expected, result);
    }

    [Fact]
    public async System.Threading.Tasks.Task NotAssigned_ManagerCheck_ReceivesAssignedUserDepartmentsPlusGeneralManagement()
    {
        List<PermissionDto>? received = null;
        GivenTask(_assignedId);
        GivenAssignedUser(UserDto(_assignedId, "المبيعات", "التسويق"));
        GivenCallerIsManager(true, p => received = p);

        await _handler.Handle(Query(), default);

        Assert.NotNull(received);
        Assert.Equal(3, received!.Count);
        Assert.Contains(received, p => p.DepartmentName == "المبيعات");
        Assert.Contains(received, p => p.DepartmentName == "التسويق");
        Assert.Contains(received, p => p.DepartmentName == GeneralManagement);
    }

    [Fact]
    public async System.Threading.Tasks.Task NotAssigned_UserHasNoPermissions_GeneralManagerStillChecked()
    {
        List<PermissionDto>? received = null;
        GivenTask(_assignedId);
        GivenAssignedUser(UserDto(_assignedId));
        GivenCallerIsManager(true, p => received = p);

        await _handler.Handle(Query(), default);

        var single = Assert.Single(received!);
        Assert.Equal(GeneralManagement, single.DepartmentName);
    }

    // ---------- Delegation to repositories ----------

    [Fact]
    public async System.Threading.Tasks.Task LoadsTaskByQueryId()
    {
        GivenTask(_callerId);

        await _handler.Handle(Query(), default);

        _tasks.Verify(r => r.GetDetailAsync(_taskId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task LoadsUserByAssignedUserId_NotByCaller_NotByTaskId()
    {
        GivenTask(_assignedId);
        GivenAssignedUser(UserDto(_assignedId));
        GivenCallerIsManager(true);

        await _handler.Handle(Query(), default);

        _users.Verify(r => r.GetDetailAsync(_assignedId, It.IsAny<CancellationToken>()), Times.Once);
        _users.Verify(r => r.GetDetailAsync(_callerId, It.IsAny<CancellationToken>()), Times.Never);
        _users.Verify(r => r.GetDetailAsync(_taskId, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async System.Threading.Tasks.Task PassesCancellationTokenToBothRepositories()
    {
        using var cts = new CancellationTokenSource();
        var expected = GivenTask(_assignedId, cts.Token);
        GivenAssignedUser(UserDto(_assignedId), cts.Token);
        GivenCallerIsManager(true);

        // If a different token were forwarded, the mocks would not match
        // (task -> null => NotFoundException, user -> null => ForbiddenException).
        var result = await _handler.Handle(Query(), cts.Token);

        Assert.Same(expected, result);
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskRepositoryException_Propagates()
    {
        _tasks.Setup(r => r.GetDetailAsync(_taskId, It.IsAny<CancellationToken>()))
              .ThrowsAsync(new InvalidOperationException("db"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(Query(), default));
    }

    [Fact]
    public async System.Threading.Tasks.Task UserRepositoryException_Propagates()
    {
        GivenTask(_assignedId);
        _users.Setup(r => r.GetDetailAsync(_assignedId, It.IsAny<CancellationToken>()))
              .ThrowsAsync(new InvalidOperationException("db"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(Query(), default));
    }
}
