using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

[AuthorizePermisions(Role = Role.Manager, Department = "الإدارة")]

public sealed record UpdateUserCommand(
    Guid Id,
    string AccountName,
    string FullName,
    string Email,
    bool? IsActive,
    List<PermissionCommand>? Permissions) : IRequest;
