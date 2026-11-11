using CRM.AlMohtaref.Domain.Entities.Contacts;

namespace CRM.AlMohtaref.Domain.Entities.Campaigns;

public class CampaignLead
{
    public Guid Id { get; private set; }

    public Guid CampaignId { get; private set; }

    public Guid StatusId { get; private set; }

    public Guid StudentId { get; private set; }

    public Campaign Campaign { get; private set; }

    public LeadStatus Status { get; private set; }

    public Student Student { get; private set; }

    private CampaignLead()
    {
        Campaign = null!;
        Status = null!;
        Student = null!;
    }

    public CampaignLead(Guid campaignId, Guid statusId, Guid studentId) : this()
    {
        CampaignId = campaignId;
        StatusId = statusId;
        StudentId = studentId;
    }
}