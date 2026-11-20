using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRM.AlMohtaref.Application.Features.Auth.Login;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Auth.RefreshToken
{
    public sealed record RefreshTokenCommand(
        string RefreshToken) : IRequest<RefreshTokenDto>;
}
