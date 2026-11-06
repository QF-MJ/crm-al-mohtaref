using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CRM.AlMohtaref.Api.Contracts.Auth;
using MediatR;
using CRM.AlMohtaref.Application.Features.Auth.Login;
using CRM.AlMohtaref.Application.Features.Auth;

namespace CRM.AlMohtaref.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class Auth : ControllerBase
    {
        private readonly ISender _sender;

        public Auth(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("login")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<ActionResult<LoginDto>> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            LoginDto login = await _sender.Send(new LoginCommand(request.AccountName, request.Password), ct);

            return Ok(login);
        }

    }
}
