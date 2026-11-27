using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Notes;

[AuthorizePermisions(Role = Role.Employee, Department = "المبيعات")]
[AuthorizePermisions(Role = Role.Manager, Department = "المبيعات")]
[AuthorizePermisions(Role = Role.Employee, Department = "خدمة العملاء")]
[AuthorizePermisions(Role = Role.Manager, Department = "خدمة العملاء")]
[AuthorizePermisions(Role = Role.Manager, Department = "الإدارة")]

public sealed record CreateNoteCommand(
    Guid ContactId,
    string Text
) : IRequest<Guid>;
