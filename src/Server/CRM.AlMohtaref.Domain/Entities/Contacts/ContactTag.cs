namespace CRM.AlMohtaref.Domain.Entities.Contacts;

// A tag attached to exactly one of: a student or a contact.
public class ContactTag
{
    public int Id { get; private set; }

    public int TagId { get; private set; }

    public int? StudentId { get; private set; }

    public int? ContactId { get; private set; }

    public Tag Tag { get; private set; }

    public Student? Student { get; private set; }

    public Contact? Contact { get; private set; }

    private ContactTag()
    {
        Tag = null!;
    }

    public static ContactTag ForStudent(int tagId, int studentId)
    {
        return new ContactTag { TagId = tagId, StudentId = studentId };
    }

    public static ContactTag ForContact(int tagId, int contactId)
    {
        return new ContactTag { TagId = tagId, ContactId = contactId };
    }
}