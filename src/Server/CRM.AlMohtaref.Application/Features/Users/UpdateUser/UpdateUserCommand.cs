using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed record UpdateUserCommand(
    int Id,
    string AccountName,
    string FullName,
    string Email,
    bool? IsActive,
    List<PermissionCommand>? Permissions) : IRequest;
