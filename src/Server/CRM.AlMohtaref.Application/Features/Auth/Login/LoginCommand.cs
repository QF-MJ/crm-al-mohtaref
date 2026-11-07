using CRM.AlMohtaref.Application.Features.Auth.Login;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Auth;

public record LoginCommand(
    string AccountName,
    string Password
) : IRequest<LoginDto>;
