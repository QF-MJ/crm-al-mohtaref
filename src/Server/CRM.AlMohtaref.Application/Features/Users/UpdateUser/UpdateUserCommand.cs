using CRM.AlMohtaref.Application.Common.Attributes;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

[AuthorizePermisions(Role = "مدير", Department = "الإدارة")]

public sealed record UpdateUserCommand(
    Guid Id,
    string AccountName,
    string FullName,
    string Email,
    bool? IsActive,
    List<PermissionCommand>? Permissions) : IRequest;
