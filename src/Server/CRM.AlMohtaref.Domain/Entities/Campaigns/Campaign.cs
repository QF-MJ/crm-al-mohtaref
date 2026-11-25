using CRM.AlMohtaref.Domain.Entities.Questions;
using CRM.AlMohtaref.Domain.Entities.Users;
using CRM.AlMohtaref.Domain.Enums;

namespace CRM.AlMohtaref.Domain.Entities.Campaigns;

public class Campaign
{
    public Guid Id { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public Guid TrackId { get; private set; }

    public string? Url { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public decimal? Budget { get; private set; }

    public Track Track { get; private set; }

    public User CreatedByUser { get; private set; }

    public CampaignStatus CampaignStatus { get; private set; }

    public ICollection<CampaignLead> Leads { get; private set; }

    public ICollection<PlatformCampaign> PlatformCampaigns { get; private set; }

    public ICollection<QuestionCampaign> QuestionCampaigns { get; private set; }

    private Campaign()
    {
        Title = string.Empty;
        Track = null!;
        CreatedByUser = null!;
        CampaignStatus = CampaignStatus.New;
        Leads = new List<CampaignLead>();
        PlatformCampaigns = new List<PlatformCampaign>();
        QuestionCampaigns = new List<QuestionCampaign>();
    }

    public Campaign(string title, Guid trackId, Guid createdByUserId, CampaignStatus campaignStatus,
        string? description = null, string? url = null, decimal? budget = null)
        : this()
    {
        Title = title;
        TrackId = trackId;
        CreatedByUserId = createdByUserId;
        CampaignStatus = campaignStatus;
        Description = description;
        Url = url;
        Budget = budget;
    }

    public void ChangeStatus(CampaignStatus newStatus)
    {
        CampaignStatus = newStatus;
    }

    public void UpdateDetails(string title, string description, CampaignStatus status, Guid trackId, string url, decimal budget)
    {
        Title = title;
        Description = description;
        CampaignStatus = status;
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
