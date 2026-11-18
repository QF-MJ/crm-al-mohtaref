using CRM.AlMohtaref.Application.Features.Auth;
using CRM.AlMohtaref.Application.Features.Auth.RefreshToken;

namespace CRM.AlMohtaref.Api.Contracts.Auth
{
    public sealed record RefreshTokenRequest(string RefreshToken, string AccountName)
    {
        public RefreshRefreshTokenCommand ToCommand() => new(RefreshToken, AccountName);
    }
    
}
