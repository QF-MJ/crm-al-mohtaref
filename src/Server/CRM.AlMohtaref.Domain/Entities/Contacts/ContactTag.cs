namespace CRM.AlMohtaref.Domain.Entities.Contacts;

// A tag attached to exactly one of: a student or a contact.
public class ContactTag
{
    public Guid Id { get; private set; }

    public Guid TagId { get; private set; }

    public Guid? StudentId { get; private set; }

    public Guid? ContactId { get; private set; }

    public Tag Tag { get; private set; }

    public Student? Student { get; private set; }

    public Contact? Contact { get; private set; }

    private ContactTag()
    {
        Tag = null!;
    }

    public static ContactTag ForStudent(Guid tagId, Guid studentId)
    {
        return new ContactTag { TagId = tagId, StudentId = studentId };
    }

    public static ContactTag ForContact(Guid tagId, Guid contactId)
    {
        return new ContactTag { TagId = tagId, ContactId = contactId };
    }
}