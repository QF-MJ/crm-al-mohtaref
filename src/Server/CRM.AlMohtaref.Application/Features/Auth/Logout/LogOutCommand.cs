using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Auth.Logout
{
    public sealed record LogoutCommand(string refreshToken) : IRequest;
    
}
