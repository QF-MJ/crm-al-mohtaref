using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CRM.AlMohtaref.Api.Contracts.Auth;
using MediatR;
using CRM.AlMohtaref.Application.Features.Auth.Login;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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

    }
}