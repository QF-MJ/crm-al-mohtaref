
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Auth.Logout
{
    public sealed record LogOutCommand(string refreshToken) : IRequest<bool>;
    
}
