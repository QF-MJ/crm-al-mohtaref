namespace CRM.AlMohtaref.Domain.Entities.Campaigns;

public class Platform
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public ICollection<PlatformCampaign> PlatformCampaigns { get; private set; }

    private Platform()
    {
        Name = string.Empty;
        PlatformCampaigns = new List<PlatformCampaign>();
    }

    public Platform(string name) : this()
    {
        Name = name;
    }
}
