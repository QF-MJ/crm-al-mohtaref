using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Application.Common.Queries;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed record GetUsersQuery(
    int PageNumber = 1,
    string? Name = null,
    string? AccountName = null,
    string? Email = null,
    bool? IsActive = null,
    int? RoleId = null,
    int? DepartmentId = null) : PagedQuery(PageNumber), IRequest<UsersPageDto>;
