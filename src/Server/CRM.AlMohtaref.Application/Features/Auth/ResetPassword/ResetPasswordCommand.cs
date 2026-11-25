
using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Application.Features.Auth.Login;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Auth.ResetPassword
{
    [AuthorizePermisions(Role = Role.Manager , Department = "الإدارة")]
    public sealed record ResetPasswordCommand(
        Guid userId , string newPassword , string confirmPassword) : IRequest<bool>;
}
