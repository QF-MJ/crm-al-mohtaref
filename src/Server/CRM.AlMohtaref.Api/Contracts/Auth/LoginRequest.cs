namespace CRM.AlMohtaref.Api.Contracts.Auth;

public sealed record LoginRequest
(
    string AccountName,
    string Password
);

