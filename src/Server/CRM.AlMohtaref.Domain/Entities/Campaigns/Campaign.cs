using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Domain.Entities.Campaigns;

public class Campaign
{
    public int Id { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public int TrackId { get; private set; }

    public string? Url { get; private set; }

    public int CreatedByUserId { get; private set; }

    public Guid CampaginStatusId { get; private set; }

    public decimal? Budget { get; private set; }

    public Track Track { get; private set; }

    public User CreatedByUser { get; private set; }

    public CampaginStatus CampaignStatus { get; private set; }

    public ICollection<CampaignLead> Leads { get; private set; }

    private Campaign()
    {
        Title = string.Empty;
        Track = null!;
        CreatedByUser = null!;
        Leads = new List<CampaignLead>();
    }

    public Campaign(string title, int trackId, int createdByUserId,
        string? description = null, string? url = null, decimal? budget = null)
        : this()
    {
        Title = title;
        TrackId = trackId;
        CreatedByUserId = createdByUserId;
        Description = description;
        Url = url;
        Budget = budget;
    }
}