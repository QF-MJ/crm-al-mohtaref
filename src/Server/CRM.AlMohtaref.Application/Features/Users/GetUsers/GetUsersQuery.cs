using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Application.Common.Queries;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

[AuthorizePermisions(Role = Domain.Entities.Users.Role.Manager, Department = "الإدارة")]
[AuthorizePermisions(Role = Domain.Entities.Users.Role.Manager, Department = "المبيعات")]
[AuthorizePermisions(Role = Domain.Entities.Users.Role.Manager, Department = "التسويق")]
[AuthorizePermisions(Role = Domain.Entities.Users.Role.Manager, Department = "التوجيه الأكاديمي")]
[AuthorizePermisions(Role = Domain.Entities.Users.Role.Manager, Department = "خدمة العملاء")]

public sealed record GetUsersQuery(
    int PageNumber = 1,
    string? Name = null,
    string? AccountName = null,
    string? Email = null,
    bool? IsActive = null,
    Role? Role = null,
    Guid? DepartmentId = null) : PagedQuery(PageNumber), IRequest<UsersPageDto>;
