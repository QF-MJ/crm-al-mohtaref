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

    public ICollection<ContactTag> ContactTags { get; private set; }

    private Student()
    {
        Contact = null!;
        CampaignLeads = new List<CampaignLead>();
        ContactTags = new List<ContactTag>();
    }

    public Student(Guid personId, Guid contactId) : this()
    {
        PersonId = personId;
        ContactId = contactId;
    }
}