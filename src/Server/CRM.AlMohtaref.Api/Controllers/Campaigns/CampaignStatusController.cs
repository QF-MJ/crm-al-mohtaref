using CRM.AlMohtaref.Api.Contracts.Users;
using CRM.AlMohtaref.Application.Features.Users.AddUser;
using CRM.AlMohtaref.Application.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.AlMohtaref.Api.Controllers.Campaigns
{
    [Route("api/campaigns/status")]
    [ApiController]
    public class CampaignStatusController : ControllerBase
    {
        private readonly ISender _sender;

        public CampaignStatusController(ISender sender)
        {
            _sender = sender;
        }

    }
}
