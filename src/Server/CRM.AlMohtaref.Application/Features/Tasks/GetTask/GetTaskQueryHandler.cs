using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Tasks.Dtos;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Tasks.GetTask;

public sealed class GetTaskQueryHandler(ITaskRepository taskRepository, IUserRepository userRepository, ICurrentUserService currentUser)
    : IOwnershipChecker<Guid>, IRequestHandler<GetTaskQuery, TaskDetailsDto>
{
    public async Task<TaskDetailsDto> Handle(GetTaskQuery query, CancellationToken ct)
    {
        var task = await taskRepository.GetDetailAsync(query.TaskId, ct);


        if (task == null)
        {
            throw new NotFoundException($"المهمة بالمعرف\"{query.TaskId}\" غير موجودة");
        }

        if (!await IsOwnerAsync(task.AssignedToUser.Id, ct))
        {
            throw new ForbiddenException($"لا تملك صلاحيات عرض مهام المستخدم بالمعرف {task.AssignedToUser.Id}");
        }

        return task;
    }

    public async Task<bool> IsOwnerAsync(Guid id, CancellationToken ct = default)
    {
        if (id == currentUser.UserId)
            return true;

        var user = await userRepository.GetDetailAsync(id, ct);

        if (user == null)
            return false;

        // Temporarily include "الإدارة" to allow general managers to pass the department manager check
        var permissionsToCheck = user.Permissions
            .Append(new PermissionDto { DepartmentName = "الإدارة" });

        return currentUser.IsManagerInAnyDepartment(permissionsToCheck);
    }
}
