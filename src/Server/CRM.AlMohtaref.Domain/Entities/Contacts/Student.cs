using CRM.AlMohtaref.Domain.Entities.Campaigns;

namespace CRM.AlMohtaref.Domain.Entities.Contacts;

public class Student : Person
{
    public Guid Id { get; private set; }

    // the guardian
    public Guid ContactId { get; private set; }

    public Contact Contact { get; private set; }

    public ICollection<CampaignLead> CampaignLeads { get; private set; }

    public ICollection<StudentTag> StudentTags { get; private set; }

    private Student()
    {
        Contact = null!;
        CampaignLeads = new List<CampaignLead>();
        StudentTags = new List<StudentTag>();
    }

    public Student(Guid contactId) : this()
    {
        ContactId = contactId;
    }
}