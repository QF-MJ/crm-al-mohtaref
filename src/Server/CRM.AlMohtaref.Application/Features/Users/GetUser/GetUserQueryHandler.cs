using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDetailDto>
{
    private readonly IUserRepository _users;

    public GetUserQueryHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<UserDetailDto> Handle(GetUserQuery request, CancellationToken ct)
    {
        var user = await _users.GetDetailAsync(request.Id, ct);

        if (user == null)
            throw new NotFoundException($"المستخدم بالمعرف {request.Id} غير موجود ");

        return user;
    }
}
