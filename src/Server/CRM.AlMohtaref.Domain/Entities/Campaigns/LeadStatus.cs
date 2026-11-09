namespace CRM.AlMohtaref.Domain.Entities.Campaigns;

public class LeadStatus
{
    public int Id { get; private set; }

    public string Name { get; private set; }

    public ICollection<CampaignLead> Leads { get; private set; }

    private LeadStatus()
    {
        Name = string.Empty;
        Leads = new List<CampaignLead>();
    }

    public LeadStatus(string name) : this()
    {
        Name = name;
    }
}