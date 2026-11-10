using CRM.AlMohtaref.Application.Common.Queries;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Contacts.GetContacts;

public record GetContactsQuery(
    int PageNumber = 1,
    string? CampaignTitle = null,
    int? TrackId = null,
    int? LeadStatusId = null,
    List<int>? TagIds = null,
    string? StudentName = null,
    string? ContactName = null,
    string? PhoneNumber = null
) : PagedQuery(PageNumber), IRequest<ContactsPageDto>;
