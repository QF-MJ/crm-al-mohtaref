

using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users.AddUser
{
    public sealed record AddUserCommand(
     string Email,
     string AccountName,
     string FullName,
     string Password,
    List<PermissionCommand>? Permissions) : IRequest<Guid>;

}
