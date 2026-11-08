using CRM.AlMohtaref.Domain.Entities.Contacts;

namespace CRM.AlMohtaref.Domain.Entities.Campaigns;

public class CampaignLead
{
    public int Id { get; private set; }

    public int CampaignId { get; private set; }

    public int StatusId { get; private set; }

    public int StudentId { get; private set; }

    public Campaign Campaign { get; private set; }

    public LeadStatus Status { get; private set; }

    public Student Student { get; private set; }

    private CampaignLead()
    {
        Campaign = null!;
        Status = null!;
        Student = null!;
    }

    public CampaignLead(int campaignId, int statusId, int studentId) : this()
    {
        CampaignId = campaignId;
        StatusId = statusId;
        StudentId = studentId;
    }
}