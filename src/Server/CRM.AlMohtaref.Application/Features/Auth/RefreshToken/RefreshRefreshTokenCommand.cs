
using CRM.AlMohtaref.Application.Features.Auth.Login;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Auth.RefreshToken
{
    public sealed record RefreshRefreshTokenCommand(
        string RefreshToken) : IRequest<RefreshTokenDto>;
}
