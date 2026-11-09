

using CRM.AlMohtaref.Application.Common.Attributes;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users.AddUser
{
    [AuthorizePermisions(Roles = "مدير")]
    public sealed record AddUserCommand(
     string Email,
     string AccountName,
     string FullName,
     string Password,
    List<PermissionCommand>? Permissions) : IRequest<Guid>;

}
