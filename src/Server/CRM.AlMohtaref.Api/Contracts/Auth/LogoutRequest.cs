

namespace CRM.AlMohtaref.Api.Contracts.Auth
{

    public sealed record LogoutRequest(string RefreshToken, string AccountName)
    {
       // public LogoutCommand ToCommand() => new(RefreshToken, AccountName);
    }
}
