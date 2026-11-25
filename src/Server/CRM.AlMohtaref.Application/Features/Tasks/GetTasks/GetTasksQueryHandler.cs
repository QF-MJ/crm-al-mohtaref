using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Tasks.DTOs;
using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Tasks.GetTasks;

public class GetTasksQueryHandler(ITaskRepository taskRepository, IUserRepository userRepository, ICurrentUserService currentUser)
    : IOwnershipChecker<UserDetailDto>, IRequestHandler<GetTasksQuery, TasksPageDto>
{
    public async Task<TasksPageDto> Handle(GetTasksQuery query, CancellationToken ct)
    {
        var user = await userRepository.GetDetailAsync(query.AssignedToUserId, ct);

        if (user == null)
            throw new NotFoundException($"المستخدم بالمعرف {query.AssignedToUserId} غير موجود ");

        if (!await IsOwnerAsync(user, ct))
            throw new ForbiddenException($"لا تملك صلاحيات عرض مهام المستخدم بالمعرف {query.AssignedToUserId}");

        return await taskRepository.SearchAsync(query, ct);
    }


    public Task<bool> IsOwnerAsync(UserDetailDto user, CancellationToken ct = default)
    {
        if (user.Id == currentUser.UserId)
            return Task.FromResult(true);

        // Temporarily include "الإدارة" to allow general managers to pass the department manager check
        var permissionsToCheck = user.Permissions
            .Append(new PermissionDto { DepartmentName = "الإدارة" });

        return Task.FromResult(currentUser.IsManagerInAnyDepartment(permissionsToCheck));
    }

}
