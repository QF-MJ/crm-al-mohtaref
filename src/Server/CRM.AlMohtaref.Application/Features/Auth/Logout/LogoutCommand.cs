
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Auth.Logout
{
    public sealed record LogoutCommand(string refreshToken) : IRequest;
    
}
