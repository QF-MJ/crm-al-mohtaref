using CRM.AlMohtaref.Api.Contracts.Lookups;
using CRM.AlMohtaref.Application.Features.Lookups.Dtos;
using CRM.AlMohtaref.Application.Features.Lookups.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.AlMohtaref.Api.Controllers
{
    [Route("api/lookups")]
    [ApiController]
    [Authorize]
    public class LookupsController(ISender sender)
        : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<LookupsDto>> GetLookups([FromQuery] GetLookupsRequest request, CancellationToken ct)
        {
            return await sender.Send(request.ToQuery(), ct);
        }

        [HttpGet("managers/{managerId:guid}/users")]
        public async Task<ActionResult<IEnumerable<LookupItemDto>>> GetUsers(Guid managerId, CancellationToken ct)
        {
            var result = await sender.Send(new GetUsersCommand(managerId), ct);
            return Ok(result);
        }

    }
}