using CRM.AlMohtaref.Application.Common.Attributes;
using MediatR;


using Role = CRM.AlMohtaref.Domain.Entities.Users.Role;
namespace CRM.AlMohtaref.Application.Features.Tasks.DeleteTask;


[AuthorizePermisions(Role = Role.Manager, Department = "الإدارة")]
[AuthorizePermisions(Role = Role.Manager, Department = "المبيعات")]
[AuthorizePermisions(Role = Role.Manager, Department = "التسويق")]
[AuthorizePermisions(Role = Role.Manager, Department = "التوجيه الأكاديمي")]
[AuthorizePermisions(Role = Role.Manager, Department = "خدمة العملاء")]
public record DeleteTaskCommand(Guid TaskId) : IRequest;
