
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CRM.AlMohtaref.Api.Contracts.Auth;
using MediatR;
using CRM.AlMohtaref.Application.Features.Auth.Login;
using Microsoft.AspNetCore.Identity.Data;
using CRM.AlMohtaref.Application.Features.Auth.RefreshToken;

namespace CRM.AlMohtaref.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(ISender sender) 
        : ControllerBase
    {

        [HttpPost("login")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<ActionResult<LoginDto>> Login([FromBody] Contracts.Auth.LoginRequest request, CancellationToken ct)
        {
            LoginDto login = await sender.Send(request.ToCommand(), ct);

            return Ok(login);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<RefreshTokenDto>> Refresh([FromBody] RefreshTokenRequest request , CancellationToken ct)
        {
            return Ok(await sender.Send(request.ToCommand(), ct));
        }

        [HttpPost("logout")]
        public async  Task<ActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
        {
            await sender.Send(request.ToCommand(), ct);

            return NoContent();
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromBody] Api.Contracts.Auth.ResetPasswordRequest request, CancellationToken ct)
        {
            await sender.Send(request.ToCommand(), ct);

            return NoContent();
        }

    }
}
