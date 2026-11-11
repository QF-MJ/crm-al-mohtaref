using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed class GetUserQueryHandler(IUserRepository userRepository, ICurrentUserService currentUser, ILookupRepository lookup) 
    : IOwnershipChecker<UserDetailDto>, IRequestHandler<GetUserQuery, UserDetailDto>
{
    public async Task<UserDetailDto> Handle(GetUserQuery request, CancellationToken ct)
    {
        var user = await userRepository.GetDetailAsync(request.Id, ct);

        if (user == null)
            throw new NotFoundException($"المستخدم بالمعرف {request.Id} غير موجود ");

        if (!await IsOwnerAsync(user, ct))
            throw new ForbiddenException($"لا تملك صلاحيات عرض المستخدم بالمعرف {request.Id}");

        return user;
    }

    public Task<bool> IsOwnerAsync(UserDetailDto user, CancellationToken ct)
    {
        bool isAllowed = currentUser.HasDepartment("الإدارة") || currentUser.UserId == user.Id;

        if (!isAllowed )
        {
            if (!currentUser.HasAnyDepartment(user.Permissions))
            {
                return Task.FromResult(false);
            }
        }

        return Task.FromResult(true);
    }
}
