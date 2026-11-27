using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Tasks.Dtos;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Tasks.GetTask;

public sealed class GetTaskQueryHandler(ITaskRepository taskRepository, IUserRepository userRepository, ICurrentUserService currentUser)
    : IOwnershipChecker<TaskDetailsDto>, IRequestHandler<GetTaskQuery, TaskDetailsDto>
{
    public async Task<TaskDetailsDto> Handle(GetTaskQuery query, CancellationToken ct)
    {
        var task = await taskRepository.GetDetailAsync(query.TaskId, ct);

        if (task == null)
        {
            throw new NotFoundException($"المهمة بالمعرف\"{query.TaskId}\" غير موجودة");
        }

        if (!await IsOwnerAsync(task, ct))
        {
            throw new ForbiddenException($"لا تملك صلاحيات عرض مهام المستخدم بالمعرف {task.AssignedToUser.Id}");
        }

        return task;
    }

    public async Task<bool> IsOwnerAsync(TaskDetailsDto task, CancellationToken ct = default)
    {
        if (task.AssignedToUser.Id == currentUser.UserId 
            || task.CreatedByUser.Id == currentUser.UserId)
            return true;

        if (currentUser.Permissions == null)
            return false;

        return currentUser.IsAdministrationManager() 
            || await userRepository.IsManagerOfUserAsync(task.AssignedToUser.Id, currentUser.Permissions, ct);
    }

}
