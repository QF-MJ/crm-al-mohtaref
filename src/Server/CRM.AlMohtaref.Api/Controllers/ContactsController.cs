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
    public class ContactsController(ISender sender) 
        : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<ContactsPageDto>> GetContacts([FromQuery] GetContactsRequest request, CancellationToken ct)
        {
            return await sender.Send(request.ToQuery(), ct);
        }
    }
}