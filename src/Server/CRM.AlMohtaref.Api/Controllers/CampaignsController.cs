using CRM.AlMohtaref.Api.Contracts.Campaigns;
using CRM.AlMohtaref.Api.Contracts.Questions;
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