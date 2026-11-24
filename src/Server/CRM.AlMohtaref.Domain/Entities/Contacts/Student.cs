using CRM.AlMohtaref.Domain.Entities.Campaigns;

namespace CRM.AlMohtaref.Domain.Entities.Contacts;

public class Student : Person
{
    public Guid Id { get; private set; }

    public Guid PersonId { get; private set; }

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

    public Student(Guid personId, Guid contactId) : this()
    {
        PersonId = personId;
        ContactId = contactId;
    }
}