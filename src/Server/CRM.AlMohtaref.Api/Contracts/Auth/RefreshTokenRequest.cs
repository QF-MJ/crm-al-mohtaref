using CRM.AlMohtaref.Application.Features.Auth;
using CRM.AlMohtaref.Application.Features.Auth.RefreshToken;

namespace CRM.AlMohtaref.Api.Contracts.Auth
{
    public sealed record RefreshTokenRequest(string RefreshToken)
    {
        public RefreshTokenCommand ToCommand() => new(RefreshToken);
    }
    
}
