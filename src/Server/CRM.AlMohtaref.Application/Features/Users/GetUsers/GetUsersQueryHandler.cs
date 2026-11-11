using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed class GetUsersQueryHandler(IUserRepository user, ICurrentUserService currentUser, ILookupRepository lookup) 
    : IOwnershipChecker<GetUsersQuery>, IRequestHandler<GetUsersQuery, UsersPageDto>
{
    public async Task<UsersPageDto> Handle(GetUsersQuery request, CancellationToken ct)
    {
       
        if(! await IsOwnerAsync(request, ct))
        {
            throw new ForbiddenException("البيانات المطلوبة لا تنتمي إلى القسم الخاص بك");
        }

        return await user.SearchAsync(request, ct);
    }

    public async Task<bool> IsOwnerAsync(GetUsersQuery request, CancellationToken ct)
    {
        bool isManager = currentUser.HasDepartment("الإدارة");

        if (!isManager)
        {
            if (!request.DepartmentId.HasValue)
            {
                return false;
            }

            var department = await lookup.GetDepartmentByIdAsync(request.DepartmentId.Value, ct);

            if (department == null || !currentUser.HasDepartment(department.Name))
            {
                return false;
            }
        }

        return true;
    }
}
