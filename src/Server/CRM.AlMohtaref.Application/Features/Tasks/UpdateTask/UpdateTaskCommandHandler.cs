using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
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

        if (task is null)
        {
            throw new NotFoundException($"المهمة بالمعرف {request.TaskId} غير موجودة");
        }

        if (!await IsOwnerAsync(task, ct))
        {
            throw new ForbiddenException("تعديل هذه المهمة متاح لمنشئها أو لمدير الإدارة أو لمدير قسم المنفذ فقط");
        }

        if (request.AssignedToUserId != task.AssignedToUserId)
        {
            await EnsureCanAssignToAsync(request.AssignedToUserId, ct);
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

        return Task.FromResult(
            task.AssignedToUser != null
                && currentUser.IsManagerInAnyDepartment(task.AssignedToUser.UserPermissions
                    .Select(p => new PermissionDto { DepartmentName = p.Department.Name }))
        );
    }


    private async Task EnsureCanAssignToAsync(Guid assigneeId, CancellationToken ct)
    {
        var assignee = await userRepository.GetDetailAsync(assigneeId, ct);

        if (assignee == null)
        {
            throw new NotFoundException($"المستخدم بالمعرف {assigneeId} غير موجود");
        }

        if (assigneeId == currentUser.UserId || currentUser.IsAdministrationManager())
            return;

        if (!currentUser.IsManagerInAnyDepartment(assignee.Permissions))
        {
            throw new ForbiddenException($"لا تملك صلاحية إسناد المهمة إلى المستخدم بالمعرف {assigneeId}");
        }
    }
}