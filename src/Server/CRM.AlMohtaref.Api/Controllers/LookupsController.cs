using CRM.AlMohtaref.Api.Contracts.Lookups;
using CRM.AlMohtaref.Application.Features.Lookups;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CRM.AlMohtaref.Api.Controllers
{
    [Route("api/lookups")]
    [ApiController]
    public class LookupsController : ControllerBase
    {
        private readonly ISender _sender;

        public LookupsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<ActionResult<LookupsDto>> GetLookups([FromQuery] GetLookupsRequest request, CancellationToken ct)
        {
            return await _sender.Send(new GetLookupsQuery(request?.Include), ct);
        }
    }
}
