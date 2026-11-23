

using CRM.AlMohtaref.Application.Features.Auth.Logout;

namespace CRM.AlMohtaref.Api.Contracts.Auth
{

    public sealed record LogoutRequest(string RefreshToken)
    {
        public LogoutCommand ToCommand() => new(RefreshToken);
    }
}
