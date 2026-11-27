using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Tasks.Dtos;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Tasks.GetTasks;

public class GetTasksQueryHandler(ITaskRepository taskRepository, IUserRepository userRepository, ICurrentUserService currentUser)
    : IOwnershipChecker<Guid>, IRequestHandler<GetTasksQuery, TasksPageDto>
{
    public async Task<TasksPageDto> Handle(GetTasksQuery query, CancellationToken ct)
    {
        if (!await userRepository.ExistsAsync(query.AssignedToUserId, ct))
        {
            throw new NotFoundException($"المستخدم بالمعرف \"{query.AssignedToUserId}\" غير موجود");
        }

        if (!await IsOwnerAsync(query.AssignedToUserId, ct))
        {
            throw new ForbiddenException($"لا تملك صلاحيات عرض مهام المستخدم بالمعرف \"{query.AssignedToUserId}\"");
        }

        return await taskRepository.SearchAsync(query, ct);
    }

    public async Task<bool> IsOwnerAsync(Guid userId, CancellationToken ct = default)
    {
        if (userId == currentUser.UserId)
            return true;

        if (currentUser.Permissions == null)
            return false;

        return currentUser.IsAdministrationManager()
            || await userRepository.IsManagerOfUserAsync(userId, currentUser.Permissions, ct);
    }

}
