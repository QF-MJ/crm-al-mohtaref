using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Contacts;

[AuthorizePermisions(Role = Role.Manager, Department = "الإدارة")]
[AuthorizePermisions(Role = Role.Manager, Department = "المبيعات")]
public sealed record GetContactQuery(Guid Id) : IRequest<ContactDetailDto>;
