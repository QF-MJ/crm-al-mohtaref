using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Application.Common.Queries;
using CRM.AlMohtaref.Domain.Entities.Campaigns;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Contacts.GetContacts;

[AuthorizePermisions(Role = Role.Manager, Department = "الإدارة")]
[AuthorizePermisions(Role = Role.Manager, Department = "المبيعات")]

public record GetContactsQuery(
    int PageNumber = 1,
    Guid? CampaignId = null,
    Guid? TrackId = null,
    LeadStatus? LeadStatus = null,
    List<Guid>? TagIds = null,
    string? StudentName = null,
    string? ContactName = null,
    string? PhoneNumber = null
) : PagedQuery(PageNumber), IRequest<ContactsPageDto>;
