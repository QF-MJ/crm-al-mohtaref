using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Tasks.UpdateTask;
using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Application.Interfaces;
using Moq;
using TaskEntity = CRM.AlMohtaref.Domain.Entities.Tasks.Task;
using TaskStatus = CRM.AlMohtaref.Domain.Entities.Tasks.TaskStatus;
using TaskType = CRM.AlMohtaref.Domain.Entities.Tasks.TaskType;
using UserEntity = CRM.AlMohtaref.Domain.Entities.Users.User;
using UserPermissionEntity = CRM.AlMohtaref.Domain.Entities.Users.UserPermission;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Tasks.UpdateTask;

// Handler = business rules only (existence + who may edit + who may be assigned + mapping + delegation).
// Input-shape / database rules (title length, dates, enum...) belong to the validator and are NOT retested here.
//
// Rules under test:
//   Edit    : creator, OR administration manager, OR manager in one of the CURRENT assignee's departments.
//             The current assignee (with UserPermissions -> Department) comes loaded WITH the task,
//             so checking it costs no extra query.
//   Reassign (only when AssignedToUserId changes): the new user is loaded once through IUserRepository.GetDetailAsync
//             (no-tracking DTO); it must exist, and unless the caller is the administration manager or assigns to
//             himself, the caller must manage one of the NEW user's departments.
public class UpdateTaskCommandHandlerTests
{
    private const string Sales = "Sales";
    private const string Marketing = "Marketing";

    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateTaskCommandHandler _handler;

    private readonly Guid _taskId = Guid.NewGuid();
    private readonly Guid _callerId = Guid.NewGuid();
    private readonly Guid _creatorId = Guid.NewGuid();
    private readonly Guid _oldAssigneeId = Guid.NewGuid();
    private readonly Guid _newAssigneeId = Guid.NewGuid();

    private static readonly DateTime OldStart = new(2026, 1, 1);
    private static readonly DateTime OldEnd = new(2026, 1, 2);
    private static readonly DateTime NewStart = new(2026, 11, 24);
    private static readonly DateTime NewEnd = new(2026, 11, 30);

    public UpdateTaskCommandHandlerTests()
    {
        _handler = new UpdateTaskCommandHandler(_tasks.Object, _users.Object, _currentUser.Object, _unitOfWork.Object);
        _currentUser.SetupGet(c => c.UserId).Returns(_callerId);
        _currentUser.Setup(c => c.IsAdministrationManager()).Returns(false);
        _currentUser.Setup(c => c.IsManagerInAnyDepartment(It.IsAny<IEnumerable<PermissionDto>>())).Returns(false);
    }

    // ---------- Helpers ----------

    // Assignee unchanged.
    private UpdateTaskCommand Command() => new(
        TaskId: _taskId,
        Title: "New title",
        Description: "New description",
        Status: TaskStatus.InProgress,
        StartTime: NewStart,
        EndTime: NewEnd,
        AssignedToUserId: _oldAssigneeId);

    // Assignee changed.
    private UpdateTaskCommand ReassignCommand() => Command() with { AssignedToUserId = _newAssigneeId };

    // User has a private constructor and private setters, and UserPermission / Department shapes are not needed
    // by the handler beyond "Department.Name", so these entities are built with reflection.
    // This is the only place that touches the entity internals.
    private static UserEntity BuildUser(Guid id, params string[] departments)
    {
        // Uses the private parameterless ctor (it initializes UserPermissions).
        var user = (UserEntity)Activator.CreateInstance(typeof(UserEntity), nonPublic: true)!;
        SetMember(user, nameof(UserEntity.Id), id);

        var departmentType = typeof(UserPermissionEntity).GetProperty("Department")!.PropertyType;

        foreach (var name in departments)
        {
            // GetUninitializedObject skips constructors: only Department.Name matters to the handler.
            var department = RuntimeHelpers.GetUninitializedObject(departmentType);
            SetMember(department, "Name", name);

            var permission = (UserPermissionEntity)RuntimeHelpers.GetUninitializedObject(typeof(UserPermissionEntity));
            SetMember(permission, "Department", department);

            user.UserPermissions.Add(permission);
        }

        return user;
    }

    // Sets a property even if its setter is private or declared in a base class; falls back to the backing field.
    private static void SetMember(object target, string name, object value)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        for (var type = target.GetType(); type is not null; type = type.BaseType)
        {
            var property = type.GetProperty(name, flags);
            if (property is null)
                continue;

            if (property.SetMethod is not null)
            {
                property.SetValue(target, value);
                return;
            }

            var field = type.GetField($"<{name}>k__BackingField", flags);
            if (field is not null)
            {
                field.SetValue(target, value);
                return;
            }
        }

        throw new InvalidOperationException($"Cannot set '{name}' on {target.GetType().Name}");
    }

    // assigneeDepartments == null  => AssignedToUser is NOT loaded with the task (null).
    private TaskEntity Task(
        Guid createdBy,
        TaskType type = TaskType.Normal,
        Guid? assignedTo = null,
        string[]? assigneeDepartments = null)
    {
        var assigneeId = assignedTo ?? _oldAssigneeId;

        return new TaskEntity
        {
            Id = _taskId,
            Title = "Old title",
            Description = "Old description",
            TaskType = type,
            TaskStatus = TaskStatus.Pending,
            StartTime = OldStart,
            EndTime = OldEnd,
            CreatedByUserId = createdBy,
            AssignedToUserId = assigneeId,
            AssignedToUser = assigneeDepartments is null ? null! : BuildUser(assigneeId, assigneeDepartments)
        };
    }

    // DTO returned by the (no-tracking) user repository for the NEW assignee.
    private static UserDetailDto UserDto(Guid id, params string[] departments) => new()
    {
        Id = id,
        Permissions = departments.Select(d => new PermissionDto { DepartmentName = d }).ToList()
    };

    private void GivenTask(TaskEntity? task) =>
        _tasks.Setup(r => r.GetByIdAsync(_taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);

    private void GivenUser(Guid id, UserDetailDto? user) =>
        _users.Setup(r => r.GetDetailAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

    private void GivenCallerIsAdministrationManager(bool value = true) =>
        _currentUser.Setup(c => c.IsAdministrationManager()).Returns(value);

    // The caller is a manager only when at least one of the permissions he is checked against
    // belongs to one of these departments (this is how the real service behaves).
    private void GivenCallerManages(string[] departments, List<List<PermissionDto>>? received = null) =>
        _currentUser
            .Setup(c => c.IsManagerInAnyDepartment(It.IsAny<IEnumerable<PermissionDto>>()))
            .Returns<IEnumerable<PermissionDto>>(p =>
            {
                var list = p.ToList();
                received?.Add(list);
                return list.Any(x => departments.Contains(x.DepartmentName));
            });

    private void VerifyNothingSaved() =>
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

    private void VerifySavedOnce() =>
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

    private void VerifyNoUserLoaded() =>
        _users.Verify(r => r.GetDetailAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

    private void VerifyNoManagerCheck() =>
        _currentUser.Verify(c => c.IsManagerInAnyDepartment(It.IsAny<IEnumerable<PermissionDto>>()), Times.Never);

    private void AssertUntouched(TaskEntity task)
    {
        Assert.Equal("Old title", task.Title);
        Assert.Equal("Old description", task.Description);
        Assert.Equal(TaskStatus.Pending, task.TaskStatus);
        Assert.Equal(OldStart, task.StartTime);
        Assert.Equal(OldEnd, task.EndTime);
        Assert.Equal(_oldAssigneeId, task.AssignedToUserId);
    }

    // ---------- Task does not exist ----------

    [Fact]
    public async System.Threading.Tasks.Task TaskNotFound_ThrowsNotFound()
    {
        GivenTask(null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(Command(), default));

        Assert.Contains(_taskId.ToString(), ex.Message);
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskNotFound_DoesNotLoadUsers_NorCheckPermissions_NorSave()
    {
        GivenTask(null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(Command(), default));

        VerifyNoUserLoaded();
        VerifyNoManagerCheck();
        _currentUser.Verify(c => c.IsAdministrationManager(), Times.Never);
        VerifyNothingSaved();
    }

    [Fact]
    public async System.Threading.Tasks.Task LoadsTaskByCommandId()
    {
        GivenTask(Task(_callerId));

        await _handler.Handle(Command(), default);

        _tasks.Verify(r => r.GetByIdAsync(_taskId, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- Edit permission: creator ----------

    [Fact]
    public async System.Threading.Tasks.Task Creator_UpdatesTask_AndSaves()
    {
        var task = Task(_callerId);
        GivenTask(task);

        await _handler.Handle(Command(), default);

        Assert.Equal("New title", task.Title);
        VerifySavedOnce();
    }

    [Fact]
    public async System.Threading.Tasks.Task Creator_SameAssignee_DoesNotLoadAnyUser_NorCheckManager()
    {
        GivenTask(Task(_callerId));

        await _handler.Handle(Command(), default);

        VerifyNoUserLoaded();
        VerifyNoManagerCheck();
    }

    [Fact]
    public async System.Threading.Tasks.Task Creator_DoesNotNeedAdministrationManagerCheck()
    {
        GivenTask(Task(_callerId));

        await _handler.Handle(Command(), default);

        _currentUser.Verify(c => c.IsAdministrationManager(), Times.Never);
    }

    // ---------- Edit permission: administration manager ----------

    [Fact]
    public async System.Threading.Tasks.Task AdministrationManager_NotCreator_UpdatesTask_AndSaves()
    {
        var task = Task(_creatorId);
        GivenTask(task);
        GivenCallerIsAdministrationManager();

        await _handler.Handle(Command(), default);

        Assert.Equal("New title", task.Title);
        VerifySavedOnce();
    }

    [Fact]
    public async System.Threading.Tasks.Task AdministrationManager_SameAssignee_DoesNotLoadAnyUser_NorCheckManager()
    {
        GivenTask(Task(_creatorId));
        GivenCallerIsAdministrationManager();

        await _handler.Handle(Command(), default);

        VerifyNoUserLoaded();
        VerifyNoManagerCheck();
    }

    // ---------- Edit permission: manager of the current assignee's department ----------

    [Fact]
    public async System.Threading.Tasks.Task DepartmentManager_OfAssigneeDepartment_UpdatesTask_AndSaves()
    {
        var task = Task(_creatorId, assigneeDepartments: new[] { Sales });
        GivenTask(task);
        GivenCallerManages(new[] { Sales });

        await _handler.Handle(Command(), default);

        Assert.Equal("New title", task.Title);
        VerifySavedOnce();
    }

    [Fact]
    public async System.Threading.Tasks.Task DepartmentManager_SameAssignee_DoesNotQueryTheUserRepository()
    {
        // The current assignee comes with the task, so no extra query is needed.
        GivenTask(Task(_creatorId, assigneeDepartments: new[] { Sales }));
        GivenCallerManages(new[] { Sales });

        await _handler.Handle(Command(), default);

        VerifyNoUserLoaded();
    }

    [Fact]
    public async System.Threading.Tasks.Task DepartmentManager_CheckReceivesCurrentAssigneeDepartments_MappedFromUserPermissions()
    {
        var received = new List<List<PermissionDto>>();
        GivenTask(Task(_creatorId, assigneeDepartments: new[] { Sales, Marketing }));
        GivenCallerManages(new[] { Sales }, received);

        await _handler.Handle(Command(), default);

        var permissions = Assert.Single(received);
        Assert.Equal(2, permissions.Count);
        Assert.Contains(permissions, p => p.DepartmentName == Sales);
        Assert.Contains(permissions, p => p.DepartmentName == Marketing);
    }

    [Fact]
    public async System.Threading.Tasks.Task DepartmentManager_OfAnotherDepartment_ThrowsForbidden()
    {
        GivenTask(Task(_creatorId, assigneeDepartments: new[] { Sales }));
        GivenCallerManages(new[] { Marketing });

        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(Command(), default));
    }

    [Fact]
    public async System.Threading.Tasks.Task NotCreator_NotAdministrationManager_NotManager_ThrowsForbidden()
    {
        GivenTask(Task(_creatorId, assigneeDepartments: new[] { Sales }));

        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(Command(), default));
    }

    [Fact]
    public async System.Threading.Tasks.Task AssigneeWithoutPermissions_NotCreator_ThrowsForbidden()
    {
        GivenTask(Task(_creatorId, assigneeDepartments: Array.Empty<string>()));
        GivenCallerManages(new[] { Sales });

        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(Command(), default));
    }

    // If the repository forgets to Include(AssignedToUser...), department managers are silently rejected.
    // This test documents that failure mode so a missing Include is easy to spot.
    [Fact]
    public async System.Threading.Tasks.Task AssignedUserNotLoadedWithTask_NotCreator_ThrowsForbidden_WithoutManagerCheck()
    {
        GivenTask(Task(_creatorId, assigneeDepartments: null));
        GivenCallerManages(new[] { Sales });

        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(Command(), default));

        VerifyNoManagerCheck();
    }

    [Fact]
    public async System.Threading.Tasks.Task Assignee_IsNotEnough_ToEdit_WhenNotCreatorNorManager()
    {
        // Documents the current rule: being the assignee does not grant edit rights.
        var task = Task(_creatorId, assignedTo: _callerId, assigneeDepartments: new[] { Sales });
        GivenTask(task);

        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(
            Command() with { AssignedToUserId = _callerId }, default));
    }

    [Fact]
    public async System.Threading.Tasks.Task Forbidden_DoesNotModifyTask_NorSave()
    {
        var task = Task(_creatorId, assigneeDepartments: new[] { Sales });
        GivenTask(task);

        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(Command(), default));

        AssertUntouched(task);
        VerifyNothingSaved();
    }

    // ---------- Reassignment ----------

    [Fact]
    public async System.Threading.Tasks.Task Reassign_Creator_ManagerOfNewUserDepartment_Updates()
    {
        var task = Task(_callerId);
        GivenTask(task);
        GivenUser(_newAssigneeId, UserDto(_newAssigneeId, Sales));
        GivenCallerManages(new[] { Sales });

        await _handler.Handle(ReassignCommand(), default);

        Assert.Equal(_newAssigneeId, task.AssignedToUserId);
        VerifySavedOnce();
    }

    [Fact]
    public async System.Threading.Tasks.Task Reassign_Creator_NewUserOutsideCallerScope_ThrowsForbidden()
    {
        var task = Task(_callerId);
        GivenTask(task);
        GivenUser(_newAssigneeId, UserDto(_newAssigneeId, Marketing));
        GivenCallerManages(new[] { Sales });

        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(ReassignCommand(), default));

        AssertUntouched(task);
        VerifyNothingSaved();
    }

    [Fact]
    public async System.Threading.Tasks.Task Reassign_Creator_NewUserNotFound_ThrowsNotFound()
    {
        var task = Task(_callerId);
        GivenTask(task);
        GivenUser(_newAssigneeId, null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(ReassignCommand(), default));

        Assert.Contains(_newAssigneeId.ToString(), ex.Message);
        AssertUntouched(task);
        VerifyNothingSaved();
    }

    [Fact]
    public async System.Threading.Tasks.Task Reassign_AdministrationManager_NewUserNotFound_ThrowsNotFound()
    {
        // The existence check applies to everyone (it prevents an FK violation).
        GivenTask(Task(_creatorId));
        GivenCallerIsAdministrationManager();
        GivenUser(_newAssigneeId, null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(ReassignCommand(), default));

        VerifyNothingSaved();
    }

    [Fact]
    public async System.Threading.Tasks.Task Reassign_AdministrationManager_AnyExistingUser_Updates_WithoutManagerCheck()
    {
        var task = Task(_creatorId);
        GivenTask(task);
        GivenCallerIsAdministrationManager();
        GivenUser(_newAssigneeId, UserDto(_newAssigneeId, Marketing));

        await _handler.Handle(ReassignCommand(), default);

        Assert.Equal(_newAssigneeId, task.AssignedToUserId);
        VerifyNoManagerCheck();
        VerifySavedOnce();
    }

    [Fact]
    public async System.Threading.Tasks.Task Reassign_ToSelf_Updates_WithoutManagerCheck()
    {
        var task = Task(_callerId);
        GivenTask(task);
        GivenUser(_callerId, UserDto(_callerId));

        await _handler.Handle(Command() with { AssignedToUserId = _callerId }, default);

        Assert.Equal(_callerId, task.AssignedToUserId);
        VerifyNoManagerCheck();
        VerifySavedOnce();
    }

    [Fact]
    public async System.Threading.Tasks.Task Reassign_ToSelf_StillRequiresTheUserToExist()
    {
        GivenTask(Task(_callerId));
        GivenUser(_callerId, null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(
            Command() with { AssignedToUserId = _callerId }, default));
    }

    [Fact]
    public async System.Threading.Tasks.Task Reassign_Creator_LoadsOnlyTheNewUser_Once()
    {
        GivenTask(Task(_callerId));
        GivenUser(_newAssigneeId, UserDto(_newAssigneeId, Sales));
        GivenCallerManages(new[] { Sales });

        await _handler.Handle(ReassignCommand(), default);

        _users.Verify(r => r.GetDetailAsync(_newAssigneeId, It.IsAny<CancellationToken>()), Times.Once);
        _users.Verify(r => r.GetDetailAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task Reassign_ManagerCheck_ReceivesNewUserDepartments()
    {
        var received = new List<List<PermissionDto>>();
        GivenTask(Task(_callerId));
        GivenUser(_newAssigneeId, UserDto(_newAssigneeId, Sales, Marketing));
        GivenCallerManages(new[] { Sales }, received);

        await _handler.Handle(ReassignCommand(), default);

        var permissions = Assert.Single(received);
        Assert.Equal(2, permissions.Count);
        Assert.Contains(permissions, p => p.DepartmentName == Sales);
        Assert.Contains(permissions, p => p.DepartmentName == Marketing);
    }

    [Fact]
    public async System.Threading.Tasks.Task Reassign_DepartmentManager_OfBothUsers_Updates_QueriesOnlyTheNewUser()
    {
        var task = Task(_creatorId, assigneeDepartments: new[] { Sales });
        GivenTask(task);
        GivenUser(_newAssigneeId, UserDto(_newAssigneeId, Sales));
        GivenCallerManages(new[] { Sales });

        await _handler.Handle(ReassignCommand(), default);

        Assert.Equal(_newAssigneeId, task.AssignedToUserId);
        _users.Verify(r => r.GetDetailAsync(_newAssigneeId, It.IsAny<CancellationToken>()), Times.Once);
        _users.Verify(r => r.GetDetailAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        VerifySavedOnce();
    }

    [Fact]
    public async System.Threading.Tasks.Task Reassign_DepartmentManager_OfOldUserOnly_ToUserOutsideScope_ThrowsForbidden()
    {
        // Prevents a Sales manager from pushing a task onto a Marketing employee.
        var task = Task(_creatorId, assigneeDepartments: new[] { Sales });
        GivenTask(task);
        GivenUser(_newAssigneeId, UserDto(_newAssigneeId, Marketing));
        GivenCallerManages(new[] { Sales });

        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(ReassignCommand(), default));

        AssertUntouched(task);
        VerifyNothingSaved();
    }

    [Fact]
    public async System.Threading.Tasks.Task Reassign_NotAllowedToEdit_ThrowsForbidden_BeforeLookingUpNewUser()
    {
        GivenTask(Task(_creatorId, assigneeDepartments: new[] { Sales }));
        GivenUser(_newAssigneeId, null);

        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(ReassignCommand(), default));

        VerifyNoUserLoaded();
    }

    // ---------- Mapping the command onto the entity ----------

    [Fact]
    public async System.Threading.Tasks.Task Success_AppliesAllEditableFields()
    {
        var task = Task(_callerId);
        GivenTask(task);
        GivenUser(_newAssigneeId, UserDto(_newAssigneeId, Sales));
        GivenCallerManages(new[] { Sales });

        await _handler.Handle(ReassignCommand(), default);

        Assert.Equal("New title", task.Title);
        Assert.Equal("New description", task.Description);
        Assert.Equal(TaskStatus.InProgress, task.TaskStatus);
        Assert.Equal(NewStart, task.StartTime);
        Assert.Equal(NewEnd, task.EndTime);
        Assert.Equal(_newAssigneeId, task.AssignedToUserId);
    }

    [Fact]
    public async System.Threading.Tasks.Task Success_NullDescription_ClearsDescription()
    {
        var task = Task(_callerId);
        GivenTask(task);

        await _handler.Handle(Command() with { Description = null }, default);

        Assert.Null(task.Description);
    }

    [Fact]
    public async System.Threading.Tasks.Task Success_DoesNotChangeIdentityTypeCreatorOrParent()
    {
        var parentId = Guid.NewGuid();
        var task = Task(_callerId, TaskType.FollowUp);
        task.ParentTaskId = parentId;
        GivenTask(task);

        await _handler.Handle(Command(), default);

        Assert.Equal(_taskId, task.Id);
        Assert.Equal(TaskType.FollowUp, task.TaskType);
        Assert.Equal(_callerId, task.CreatedByUserId);
        Assert.Equal(parentId, task.ParentTaskId);
    }

    [Fact]
    public async System.Threading.Tasks.Task Success_ByAnotherUser_DoesNotChangeCreator()
    {
        var task = Task(_creatorId);
        GivenTask(task);
        GivenCallerIsAdministrationManager();

        await _handler.Handle(Command(), default);

        Assert.Equal(_creatorId, task.CreatedByUserId);
    }

    // ---------- Task kinds / statuses ----------

    public static IEnumerable<object[]> UpdatableTypes() =>
        Enum.GetValues<TaskType>()
            .Where(t => t != TaskType.SubTask)
            .Select(t => new object[] { t });

    [Theory]
    [MemberData(nameof(UpdatableTypes))]
    public async System.Threading.Tasks.Task AnyNonSubTaskType_IsUpdated(TaskType type)
    {
        var task = Task(_callerId, type);
        GivenTask(task);

        await _handler.Handle(Command(), default);

        Assert.Equal("New title", task.Title);
        VerifySavedOnce();
    }

    public static IEnumerable<object[]> AllStatuses() =>
        Enum.GetValues<TaskStatus>().Select(s => new object[] { s });

    [Theory]
    [MemberData(nameof(AllStatuses))]
    public async System.Threading.Tasks.Task AnyStatus_IsApplied(TaskStatus status)
    {
        var task = Task(_callerId);
        GivenTask(task);

        await _handler.Handle(Command() with { Status = status }, default);

        Assert.Equal(status, task.TaskStatus);
    }

    // ---------- Delegation ----------

    [Fact]
    public async System.Threading.Tasks.Task PassesCancellationToken_ToRepositories_AndSave()
    {
        using var cts = new CancellationTokenSource();
        var task = Task(_creatorId, assigneeDepartments: new[] { Sales });
        _tasks.Setup(r => r.GetByIdAsync(_taskId, cts.Token)).ReturnsAsync(task);
        _users.Setup(r => r.GetDetailAsync(_newAssigneeId, cts.Token)).ReturnsAsync(UserDto(_newAssigneeId, Sales));
        GivenCallerManages(new[] { Sales });

        // If a different token were forwarded, the mocks would not match
        // (task -> null => NotFoundException, user -> null => NotFoundException).
        await _handler.Handle(ReassignCommand(), cts.Token);

        Assert.Equal(_newAssigneeId, task.AssignedToUserId);
        _unitOfWork.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task Success_EntityIsUpdatedBeforeSaving()
    {
        var task = Task(_callerId);
        GivenTask(task);
        string? titleAtSave = null;
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .Callback(() => titleAtSave = task.Title);

        await _handler.Handle(Command(), default);

        Assert.Equal("New title", titleAtSave);
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskRepositoryException_Propagates()
    {
        _tasks.Setup(r => r.GetByIdAsync(_taskId, It.IsAny<CancellationToken>()))
              .ThrowsAsync(new InvalidOperationException("db"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(Command(), default));

        VerifyNothingSaved();
    }

    [Fact]
    public async System.Threading.Tasks.Task UserRepositoryException_Propagates_AndDoesNotSave()
    {
        var task = Task(_callerId);
        GivenTask(task);
        _users.Setup(r => r.GetDetailAsync(_newAssigneeId, It.IsAny<CancellationToken>()))
              .ThrowsAsync(new InvalidOperationException("db"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(ReassignCommand(), default));

        AssertUntouched(task);
        VerifyNothingSaved();
    }

    [Fact]
    public async System.Threading.Tasks.Task SaveChangesException_Propagates()
    {
        GivenTask(Task(_callerId));
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("db"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(Command(), default));
    }
}