using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Application.Common.Queries;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

[AuthorizePermisions(Role = "مدير" , Department = "الإدارة")]
[AuthorizePermisions(Role = "مدير" , Department = "المبيعات")]
[AuthorizePermisions(Role = "مدير" , Department = "التسويق")]
[AuthorizePermisions(Role = "مدير" , Department = "التوجيه الأكاديمي")]
[AuthorizePermisions(Role = "مدير" , Department = "خدمة العملاء")]

public sealed record GetUsersQuery(
    int PageNumber = 1,
    string? Name = null,
    string? AccountName = null,
    string? Email = null,
    bool? IsActive = null,
    Guid? RoleId = null,
    Guid? DepartmentId = null) : PagedQuery(PageNumber), IRequest<UsersPageDto>;
