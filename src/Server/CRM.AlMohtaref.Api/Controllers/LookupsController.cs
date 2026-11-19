using CRM.AlMohtaref.Api.Contracts.Lookups;
using CRM.AlMohtaref.Application.Features.Lookups.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CRM.AlMohtaref.Api.Controllers
{
    [Route("api/lookups")]
    [ApiController]
    public class LookupsController(ISender sender)
        : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<LookupsDto>> GetLookups([FromQuery] GetLookupsRequest request, CancellationToken ct)
        {
            return await sender.Send(request.ToQuery(), ct);
        }
    }
}