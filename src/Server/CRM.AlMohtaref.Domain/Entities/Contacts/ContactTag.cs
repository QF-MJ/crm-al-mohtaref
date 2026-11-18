namespace CRM.AlMohtaref.Domain.Entities.Contacts;

public class ContactTag
{
    public Guid Id { get; private set; }

    public Guid TagId { get; private set; }

    public Guid? ContactId { get; private set; }

    public Tag Tag { get; private set; }

    public Contact? Contact { get; private set; }

    private ContactTag()
    {
        Tag = null!;
    }
}