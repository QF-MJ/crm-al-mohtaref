using CRM.AlMohtaref.Domain.Entities.Contacts;

namespace CRM.AlMohtaref.Domain.Entities.Campaigns;

public class CampaignLead
{
    public Guid Id { get; private set; }

    public Guid CampaignId { get; private set; }

    public Guid PlatformCampaignId { get; private set; }

    public Guid StudentId { get; private set; }

    public Campaign Campaign { get; private set; }

    public PlatformCampaign PlatformCampaign { get; private set; }

    public LeadStatus Status { get; private set; }

    public Student Student { get; private set; }

    private CampaignLead()
    {
        Campaign = null!;
        PlatformCampaign = null!;
        Status = LeadStatus.New;
        Student = null!;
    }

    public CampaignLead(Guid campaignId, Guid platformCampaignId, LeadStatus status, Guid studentId) : this()
    {
        CampaignId = campaignId;
        PlatformCampaignId = platformCampaignId;
        Status = status;
        StudentId = studentId;
    }

    public void ChangeStatus(LeadStatus status)
    {
        Status = status;
    }
}
