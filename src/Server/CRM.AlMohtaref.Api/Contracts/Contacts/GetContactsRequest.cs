namespace CRM.AlMohtaref.Api.Contracts.Contacts;

public sealed class GetContactsRequest : PagedRequest
{
    public string? CampaignTitle { get; set; }

    public int? TrackId { get; set; }

    public int? LeadStatusId { get; set; }

    public List<int>? TagIds { get; set; }

    public string? StudentName { get; set; }

    public string? ContactName { get; set; }

    public string? PhoneNumber { get; set; }
}
