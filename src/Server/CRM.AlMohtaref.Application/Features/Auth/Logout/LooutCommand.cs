
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Auth.Logout
{
    public sealed record LooutCommand(string refreshToken) : IRequest;
    
}
