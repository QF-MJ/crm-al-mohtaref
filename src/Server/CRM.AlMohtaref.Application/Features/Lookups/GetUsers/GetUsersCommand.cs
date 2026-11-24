using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Application.Features.Lookups.Dtos;
using MediatR;

using Role = CRM.AlMohtaref.Domain.Entities.Users.Role;

namespace CRM.AlMohtaref.Application.Features.Lookups.GetUsers;

[AuthorizePermisions(Role = Role.Manager, Department = "الإدارة")]
[AuthorizePermisions(Role = Role.Manager, Department = "المبيعات")]
[AuthorizePermisions(Role = Role.Manager, Department = "التسويق")]
[AuthorizePermisions(Role = Role.Manager, Department = "التوجيه الأكاديمي")]
[AuthorizePermisions(Role = Role.Manager, Department = "خدمة العملاء")]
public sealed record GetUsersCommand(Guid ManagerId) : IRequest<IEnumerable<LookupItemDto>>;
