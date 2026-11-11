using Azure.Core;
using CRM.AlMohtaref.Api.Contracts.Contacts;
using CRM.AlMohtaref.Application.Features.Contacts.GetContacts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.AlMohtaref.Api.Controllers
{
    [Route("api/contacts")]
    [ApiController]
    [Authorize]
    public class ContactsController : ControllerBase
    {
        private readonly ISender _sender;

        public ContactsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<ActionResult<ContactsPageDto>> GetContacts([FromQuery] GetContactsRequest request, CancellationToken ct)
        {

            var query = new GetContactsQuery(
                                request.PageNumber,
                                request.CampaignTitle,
                                request.TrackId,
                                request.LeadStatusId,
                                request.TagIds,
                                request.StudentName,
                                request.ContactName,
                                request.PhoneNumber
                            );

            return await _sender.Send(query, ct);
        }
    }
}
