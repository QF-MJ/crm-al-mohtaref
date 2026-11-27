using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Users;
using CRM.AlMohtaref.Domain.Extensions;
using MediatR;

using TaskEntity = CRM.AlMohtaref.Domain.Entities.Tasks.Task;

namespace CRM.AlMohtaref.Application.Features.Tasks.UpdateTask;

public sealed class UpdateTaskCommandHandler(ITaskRepository taskRepository, IUserRepository userRepository, 
    ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    : IOwnershipChecker<TaskEntity>, IRequestHandler<UpdateTaskCommand>
{
    public async Task Handle(UpdateTaskCommand request, CancellationToken ct)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId, ct);

        if (task == null)
        {
            throw new NotFoundException($"المهمة بالمعرف {request.TaskId} غير موجودة");
        }

        if (!await IsOwnerAsync(task, ct))
        {
            throw new ForbiddenException("تعديل هذه المهمة متاح لمنشئها أو لمدير الإدارة أو لمدير قسم المنفذ فقط");
        }

        if (request.AssignedToUserId != task.AssignedToUserId 
            && !await CanAssignTaskToAsync(request.AssignedToUserId, ct))
        {
            throw new ForbiddenException("لا يمكنك إسناد هذه المهمة إلا لنفسك أو لأحد الموظفين التابعين لقسمك");
        }

        task.Update(
            request.Title,
            request.Description,
            request.Status,
            request.StartTime,
            request.EndTime,
            request.AssignedToUserId
        );

        await unitOfWork.SaveChangesAsync(ct);
    }

    public Task<bool> IsOwnerAsync(TaskEntity task, CancellationToken ct = default)
    {
        if (task.CreatedByUserId == currentUser.UserId || currentUser.IsAdministrationManager())
            return Task.FromResult(true);

        var permissions = task.AssignedToUser.UserPermissions
                    .Select(p => new PermissionDto { DepartmentName = p.Department.Name });

        return Task.FromResult(currentUser.IsManagerInAnyDepartment(permissions));
    }


    private async Task<bool> CanAssignTaskToAsync(Guid assigneeId, CancellationToken ct)
    {
        if (assigneeId == currentUser.UserId)
            return true;

        if (currentUser.Permissions == null)
            return false;

        return currentUser.IsAdministrationManager()
            || await userRepository.IsManagerOfUserAsync(assigneeId, currentUser.Permissions, ct);
    }
}