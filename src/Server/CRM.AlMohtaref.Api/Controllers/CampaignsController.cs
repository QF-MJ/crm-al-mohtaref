using CRM.AlMohtaref.Api.Contracts.Campaigns;
using CRM.AlMohtaref.Api.Contracts.Questions;
using CRM.AlMohtaref.Application.Features.Campaigns.Dtos;
using CRM.AlMohtaref.Application.Features.Campaigns.GetCampaign;
using CRM.AlMohtaref.Application.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.AlMohtaref.Api.Controllers
{
    [Route("api/campaigns")]
    [ApiController]
    public class CampaignsController(ISender sender)
        : ControllerBase
    {


        [HttpPost]
        public async Task<ActionResult<Guid>> CreateCampaign([FromBody] CreateCampaignRequest request, CancellationToken ct)
        {
            var command = request.ToCommand();
            var campaignId = await sender.Send(command, ct);
            return CreatedAtAction(nameof(GetCampaign), new { id = campaignId }, campaignId);
        }

        [HttpGet("{id:Guid}")]
        public async Task<ActionResult<CampaignDto>> GetCampaign(Guid id, CancellationToken ct)
        {
            return Ok(await sender.Send(new GetCampaignQuery(id), ct));
        }

        [HttpGet]
        public async Task<ActionResult<List<CampaignListItemDto>>> GetCampaigns([FromQuery]GetCampaignsRequest request , CancellationToken ct)
        {
            return Ok(await sender.Send( request.ToQuery(), ct));
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> UpadteCampaign(Guid id, [FromBody] UpdateCampaignRequest request, CancellationToken ct)
        {
            await sender.Send(request.ToCommand(id), ct);

            return NoContent();
        }

        [HttpPut("{id:Guid}/questions/{questionId:Guid}")]
        public async Task<IActionResult> UpdateQuestion(Guid id, Guid questionId, [FromBody] UpdateQuestionRequest request, CancellationToken ct)
        {
            await sender.Send(request.ToCommand(id, questionId), ct);

            return NoContent();
        }

        
    }
}