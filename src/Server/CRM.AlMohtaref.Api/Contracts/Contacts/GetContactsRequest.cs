using CRM.AlMohtaref.Application.Features.Contacts.GetContacts;
using CRM.AlMohtaref.Domain.Entities.Campaigns;

namespace CRM.AlMohtaref.Api.Contracts.Contacts;

public sealed class GetContactsRequest : PagedRequest
{
    public Guid? CampaignId { get; set; }

    public Guid? TrackId { get; set; }

    public LeadStatus? LeadStatus { get; set; }

    public List<Guid>? TagIds { get; set; }

    public string? StudentName { get; set; }

    public string? ContactName { get; set; }

    public string? PhoneNumber { get; set; }

    public GetContactsQuery ToQuery() => new(
        PageNumber,
        CampaignId,
        TrackId,
        LeadStatus,
        TagIds,
        StudentName,
        ContactName,
        PhoneNumber
    );
}