using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Tasks.DeleteTask;
using CRM.AlMohtaref.Application.Interfaces;
using Moq;
using TaskEntity = CRM.AlMohtaref.Domain.Entities.Tasks.Task;
using TaskType = CRM.AlMohtaref.Domain.Entities.Tasks.TaskType;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Tasks.DeleteTask;

// Handler = business rules only (existence + ownership + task-kind conflicts + delegation to repository / unit of work).
// Input-shape rules (empty id) belong to the validator and are NOT retested here.
public class DeleteTaskCommandHandlerTests
{
    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeleteTaskCommandHandler _handler;

    private readonly Guid _taskId = Guid.NewGuid();
    private readonly Guid _callerId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();

    public DeleteTaskCommandHandlerTests()
    {
        _handler = new DeleteTaskCommandHandler(_tasks.Object);
        _currentUser.SetupGet(c => c.UserId).Returns(_callerId);
        _currentUser.Setup(c => c.IsAdministrationManager()).Returns(false);
    }

    private DeleteTaskCommand Command() => new(_taskId);

    private TaskEntity Task(Guid createdBy, TaskType type = TaskType.Normal, int subTasks = 0)
    {
        var task = new TaskEntity { Id = _taskId, TaskType = type, CreatedByUserId = createdBy };

        for (var i = 0; i < subTasks; i++)
            task.SubTasks.Add(new TaskEntity { Id = Guid.NewGuid(), TaskType = TaskType.SubTask, ParentTaskId = _taskId });

        return task;
    }

    private void GivenTask(TaskEntity? task) =>
        _tasks.Setup(r => r.GetByIdAsync(_taskId, CancellationToken.None)).ReturnsAsync(task);

    private void GivenCallerIsAdministrationManager(bool value = true) =>
        _currentUser.Setup(c => c.IsAdministrationManager()).Returns(value);

    private void VerifyNothingDeleted()
    {
        _tasks.Verify(r => r.DeleteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
    public async System.Threading.Tasks.Task TaskNotFound_DoesNotDelete_NorSave()
    {
        GivenTask(null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(Command(), default));

        VerifyNothingDeleted();
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskNotFound_DoesNotCheckAdministrationManager()
    {
        GivenTask(null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(Command(), default));

        _currentUser.Verify(c => c.IsAdministrationManager(), Times.Never);
    }

    [Fact]
    public async System.Threading.Tasks.Task LoadsTaskByCommandId()
    {
        GivenTask(Task(_callerId));

        await _handler.Handle(Command(), default);

        _tasks.Verify(r => r.GetByIdAsync(_taskId, CancellationToken.None), Times.Once);
    }

    // ---------- Ownership ----------

    [Fact]
    public async System.Threading.Tasks.Task NotOwner_NotAdministrationManager_ThrowsForbidden()
    {
        GivenTask(Task(_otherUserId));

        var ex = await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(Command(), default));

        Assert.Contains(_taskId.ToString(), ex.Message);
    }

    [Fact]
    public async System.Threading.Tasks.Task NotOwner_NotAdministrationManager_DoesNotDelete_NorSave()
    {
        GivenTask(Task(_otherUserId));

        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(Command(), default));

        VerifyNothingDeleted();
    }

    [Fact]
    public async System.Threading.Tasks.Task Owner_DeletesTask_AndSaves()
    {
        var task = Task(_callerId);
        GivenTask(task);

        await _handler.Handle(Command(), default);

        _tasks.Verify(r => r.DeleteByIdAsync(_taskId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task Owner_DoesNotNeedAdministrationManagerCheck()
    {
        GivenTask(Task(_callerId));

        await _handler.Handle(Command(), default);

        _currentUser.Verify(c => c.IsAdministrationManager(), Times.Never);
    }

    [Fact]
    public async System.Threading.Tasks.Task NotOwner_AdministrationManager_DeletesTask_AndSaves()
    {
        var task = Task(_otherUserId);
        GivenTask(task);
        GivenCallerIsAdministrationManager();

        await _handler.Handle(Command(), default);

        _tasks.Verify(r => r.DeleteByIdAsync(_taskId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task NotOwner_ChecksAdministrationManager()
    {
        GivenTask(Task(_otherUserId));

        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(Command(), default));

        _currentUser.Verify(c => c.IsAdministrationManager(), Times.Once);
    }

    // ---------- Task kinds that can be deleted ----------

    public static IEnumerable<object[]> DeletableTypes() =>
        Enum.GetValues<TaskType>()
            .Where(t => t != TaskType.SubTask)
            .Select(t => new object[] { t });

    [Theory]
    [MemberData(nameof(DeletableTypes))]
    public async System.Threading.Tasks.Task AnyNonSubTaskType_WithoutSubTasks_IsDeleted(TaskType type)
    {
        var task = Task(_callerId, type);
        GivenTask(task);

        await _handler.Handle(Command(), default);

        _tasks.Verify(r => r.DeleteByIdAsync(_taskId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- Sub-task cannot be deleted ----------

    [Fact]
    public async System.Threading.Tasks.Task SubTask_ThrowsConflict()
    {
        GivenTask(Task(_callerId, TaskType.SubTask));

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(Command(), default));

        Assert.Contains(_taskId.ToString(), ex.Message);
    }

    [Fact]
    public async System.Threading.Tasks.Task SubTask_DoesNotDelete_NorSave()
    {
        GivenTask(Task(_callerId, TaskType.SubTask));

        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(Command(), default));

        VerifyNothingDeleted();
    }

    [Fact]
    public async System.Threading.Tasks.Task SubTask_AdministrationManager_StillThrowsConflict()
    {
        GivenTask(Task(_otherUserId, TaskType.SubTask));
        GivenCallerIsAdministrationManager();

        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(Command(), default));

        VerifyNothingDeleted();
    }

    // ---------- Task with sub-tasks cannot be deleted ----------

    [Fact]
    public async System.Threading.Tasks.Task TaskWithSubTasks_ThrowsConflict()
    {
        GivenTask(Task(_callerId, TaskType.Contact, subTasks: 2));

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(Command(), default));

        Assert.Contains(_taskId.ToString(), ex.Message);
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskWithSingleSubTask_ThrowsConflict()
    {
        GivenTask(Task(_callerId, TaskType.Normal, subTasks: 1));

        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(Command(), default));
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskWithSubTasks_DoesNotDelete_NorSave()
    {
        GivenTask(Task(_callerId, TaskType.Normal, subTasks: 3));

        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(Command(), default));

        VerifyNothingDeleted();
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskWithSubTasks_AdministrationManager_StillThrowsConflict()
    {
        GivenTask(Task(_otherUserId, TaskType.Normal, subTasks: 1));
        GivenCallerIsAdministrationManager();

        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(Command(), default));

        VerifyNothingDeleted();
    }

    // ---------- Order of checks ----------

    [Fact]
    public async System.Threading.Tasks.Task NotOwner_SubTask_ThrowsForbidden_NotConflict()
    {
        GivenTask(Task(_otherUserId, TaskType.SubTask));

        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(Command(), default));
    }

    [Fact]
    public async System.Threading.Tasks.Task NotOwner_TaskWithSubTasks_ThrowsForbidden_NotConflict()
    {
        GivenTask(Task(_otherUserId, TaskType.Normal, subTasks: 2));

        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(Command(), default));
    }

    // ---------- Delegation ----------

    [Fact]
    public async System.Threading.Tasks.Task PassesCancellationToken_ToDelete_AndSave()
    {
        using var cts = new CancellationTokenSource();
        var task = Task(_callerId);
        GivenTask(task);

        await _handler.Handle(Command(), cts.Token);

        _tasks.Verify(r => r.DeleteByIdAsync(_taskId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task Success_DeletesBeforeSaving()
    {
        var calls = new List<string>();
        GivenTask(Task(_callerId));
        _tasks.Setup(r => r.DeleteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
              .Callback(() => calls.Add("delete"));
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .Callback(() => calls.Add("save"));

        await _handler.Handle(Command(), default);

        Assert.Equal(new[] { "delete", "save" }, calls);
    }

    [Fact]
    public async System.Threading.Tasks.Task RepositoryException_Propagates()
    {
        _tasks.Setup(r => r.GetByIdAsync(_taskId, CancellationToken.None)).ThrowsAsync(new InvalidOperationException("db"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(Command(), default));

        VerifyNothingDeleted();
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
