using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;
using TaskStatus = CRM.AlMohtaref.Domain.Entities.Tasks.TaskStatus;

namespace CRM.AlMohtaref.Application.Features.Tasks.UpdateTask;


[AuthorizePermisions(Role = Role.Manager, Department = "الإدارة")]
[AuthorizePermisions(Role = Role.Manager, Department = "المبيعات")]
[AuthorizePermisions(Role = Role.Manager, Department = "التسويق")]
[AuthorizePermisions(Role = Role.Manager, Department = "التوجيه الأكاديمي")]
[AuthorizePermisions(Role = Role.Manager, Department = "خدمة العملاء")]
public record UpdateTaskCommand(
    Guid TaskId,
    string Title,
    string? Description,
    TaskStatus Status,
    DateTime StartTime,
    DateTime EndTime,
    Guid AssignedToUserId
) : IRequest;
