using CRM.AlMohtaref.Application.Features.Contacts.GetContacts;

namespace CRM.AlMohtaref.Api.Contracts.Contacts;

public sealed class GetContactsRequest : PagedRequest
{
    public string? CampaignTitle { get; set; }

    public Guid? TrackId { get; set; }

    public Guid? LeadStatusId { get; set; }

    public List<Guid>? TagIds { get; set; }

    public string? StudentName { get; set; }

    public string? ContactName { get; set; }

    public string? PhoneNumber { get; set; }

    public GetContactsQuery ToQuery() => new(
        PageNumber,
        CampaignTitle,
        TrackId,
        LeadStatusId,
        TagIds,
        StudentName,
        ContactName,
        PhoneNumber
    );
}