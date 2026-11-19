namespace CRM.AlMohtaref.Domain.Entities.Campaigns;

// A campaign promoted on a specific platform (Facebook, Instagram, ...)
public class PlatformCampaign
{
    public Guid Id { get; private set; }

    public Guid PlatformId { get; private set; }

    public Guid CampaignId { get; private set; }

    public Platform Platform { get; private set; }

    public Campaign Campaign { get; private set; }

    public ICollection<CampaignLead> Leads { get; private set; }

    private PlatformCampaign()
    {
        Platform = null!;
        Campaign = null!;
        Leads = new List<CampaignLead>();
    }

    public PlatformCampaign(Guid platformId, Guid campaignId) : this()
    {
        PlatformId = platformId;
        CampaignId = campaignId;
    }
}
