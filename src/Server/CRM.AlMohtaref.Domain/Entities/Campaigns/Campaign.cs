using CRM.AlMohtaref.Domain.Entities.Questions;
using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Domain.Entities.Campaigns;

public class Campaign
{
    public Guid Id { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public Guid TrackId { get; private set; }

    public string? Url { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public Guid CampaginStatusId { get; private set; }

    public decimal? Budget { get; private set; }

    public Track Track { get; private set; }

    public User CreatedByUser { get; private set; }

    public CampaginStatus CampaignStatus { get; private set; }

    public ICollection<CampaignLead> Leads { get; private set; }

    public ICollection<PlatformCampaign> PlatformCampaigns { get; private set; }

    public ICollection<QuestionCampaign> QuestionCampaigns { get; private set; }

    private Campaign()
    {
        Title = string.Empty;
        Track = null!;
        CreatedByUser = null!;
        CampaignStatus = null!;
        Leads = new List<CampaignLead>();
        PlatformCampaigns = new List<PlatformCampaign>();
        QuestionCampaigns = new List<QuestionCampaign>();
    }

    public Campaign(string title, Guid trackId, Guid createdByUserId, Guid campaignStatusId,
        string? description = null, string? url = null, decimal? budget = null)
        : this()
    {
        Title = title;
        TrackId = trackId;
        CreatedByUserId = createdByUserId;
        CampaginStatusId = campaignStatusId;
        Description = description;
        Url = url;
        Budget = budget;
    }

    public void ChangeStatus(Guid campaignStatusId)
    {
        CampaginStatusId = campaignStatusId;
    }

    public void UpdateDetails(string title, string description, Guid statusId, Guid trackId, string url, decimal budget)
    {
        Title = title;
        Description = description;
        CampaginStatusId = statusId;
        TrackId = trackId;
        Url = url;
        Budget = budget;
    }

    public void SynchronizePlatforms(IEnumerable<Guid> platformIds)
    {
        var targetIds = platformIds.Distinct()
            .ToHashSet();

        var toRemove = PlatformCampaigns
            .Where(cp => !targetIds.Contains(cp.PlatformId))
            .ToList();

        foreach (var item in toRemove)
        {
            PlatformCampaigns.Remove(item);
        }

        var existingIds = PlatformCampaigns.Select(cp => cp.PlatformId).ToHashSet();
        var toAdd = targetIds.Where(id => !existingIds.Contains(id));

        foreach (var platformId in toAdd)
        {
            PlatformCampaigns.Add(new PlatformCampaign(platformId, Id));
        }
    }

}
