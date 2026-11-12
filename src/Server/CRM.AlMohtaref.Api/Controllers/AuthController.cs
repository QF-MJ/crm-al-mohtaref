using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CRM.AlMohtaref.Api.Contracts.Auth;
using MediatR;
using CRM.AlMohtaref.Application.Features.Auth.Login;

namespace CRM.AlMohtaref.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(ISender sender) 
        : ControllerBase
    {

        [HttpPost("login")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<ActionResult<LoginDto>> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            LoginDto login = await sender.Send(request.ToCommand(), ct);

            return Ok(login);
        }

    }
}