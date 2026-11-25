

using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

[AuthorizePermisions(Role = Role.Manager, Department = "الإدارة")]

public sealed record CreateUserCommand(
 string Email,
 string AccountName,
 string FullName,
 string Password,
List<PermissionCommand>? Permissions) : IRequest<Guid>;
