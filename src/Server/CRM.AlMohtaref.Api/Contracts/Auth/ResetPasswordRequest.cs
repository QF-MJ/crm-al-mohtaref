using CRM.AlMohtaref.Application.Features.Auth.RefreshToken;
using CRM.AlMohtaref.Application.Features.Auth.ResetPassword;

namespace CRM.AlMohtaref.Api.Contracts.Auth
{
    public sealed record ResetPasswordRequest(Guid userId, string NewPassword, string ConfirmPassword)
    {
        public ResetPasswordCommand ToCommand() => new(userId , NewPassword , ConfirmPassword);
    }
}
