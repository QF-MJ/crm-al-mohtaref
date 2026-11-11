using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Application.Common.Queries;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Contacts.GetContacts;

[AuthorizePermisions(Role = "مدير", Department = "الإدارة")]
[AuthorizePermisions(Role = "مدير", Department = "المبيعات")]

public record GetContactsQuery(
    int PageNumber = 1,
    string? CampaignTitle = null,
    Guid? TrackId = null,
    Guid? LeadStatusId = null,
    List<Guid>? TagIds = null,
    string? StudentName = null,
    string? ContactName = null,
    string? PhoneNumber = null
) : PagedQuery(PageNumber), IRequest<ContactsPageDto>;
