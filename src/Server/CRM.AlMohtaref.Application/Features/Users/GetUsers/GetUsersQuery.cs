using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed record GetUsersQuery(
    int PageNumber = 1,
    string? Name = null,
    string? AccountName = null,
    string? Email = null,
    bool? IsActive = null,
    int? PermissionId = null) : IRequest<UsersPageDto>;
