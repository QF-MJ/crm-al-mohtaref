using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

using TaskEntity = CRM.AlMohtaref.Domain.Entities.Tasks.Task;
using TaskType = CRM.AlMohtaref.Domain.Entities.Tasks.TaskType;

namespace CRM.AlMohtaref.Application.Features.Tasks.DeleteTask;

public class DeleteTaskCommandHandler(ITaskRepository taskRepository, ICurrentUserService currentUser, IUnitOfWork unitOfWork) 
    : IOwnershipChecker<TaskEntity>, IRequestHandler<DeleteTaskCommand>
{
    public async Task Handle(DeleteTaskCommand command, CancellationToken ct)
    {
        var task = await taskRepository.GetByIdAsync(command.TaskId);

        if (task == null)
        {
            throw new NotFoundException($"المهمة بالمعرف\"{command.TaskId}\" غير موجودة");
        }

        if (!await IsOwnerAsync(task, ct))
        {
            throw new ForbiddenException($"لا تملك صلاحيات حذف المهمة بالمعرف\"{command.TaskId}\"");
        }

        if (task.TaskType == TaskType.SubTask)
        {
            throw new ConflictException($"لا يمكن حذف المهمة الفرعية بالمعرّف \"{command.TaskId}\" لأنها مرتبطة بسجلات الطلاب");
        }

        if (task.SubTasks.Any())
        {
            throw new ConflictException($"لا يمكن حذف المهمة بالمعرّف \"{command.TaskId}\" لوجود مهام فرعية مرتبطة بها");
        }

        taskRepository.Delete(task, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public Task<bool> IsOwnerAsync(TaskEntity task, CancellationToken ct)
    {
        return Task.FromResult(task.CreatedByUserId == currentUser.UserId 
            || currentUser.IsAdministrationManager());
    }
}
