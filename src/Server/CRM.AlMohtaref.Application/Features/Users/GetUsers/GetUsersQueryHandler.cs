using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, UsersPageDto>
{
    private readonly IUserRepository _users;

    public GetUsersQueryHandler(IUserRepository users)
    {
        _users = users;
    }

    public Task<UsersPageDto> Handle(GetUsersQuery request, CancellationToken ct)
    {
        return _users.SearchAsync(request, ct);
    }
}
