using CRM.AlMohtaref.Application.Features.Auth;

namespace CRM.AlMohtaref.Api.Contracts.Auth;

public sealed record LoginRequest(string AccountName, string Password)
{
    public LoginCommand ToCommand() => new(AccountName, Password);
}