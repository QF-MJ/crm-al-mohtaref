namespace CRM.AlMohtaref.Domain.Entities.Contacts;

public class Contact : Person
{
    public Guid Id { get; private set; }

    public Guid PersonId { get; private set; }

    public string PhoneNumber { get; private set; }


    // students that have this contact as their guardian
    public ICollection<Student> Students { get; private set; }

    public ICollection<ContactTag> ContactTags { get; private set; }

    public ICollection<Note> Notes { get; set; } = new List<Note>();

    private Contact()
    {
        PhoneNumber = string.Empty;
        Students = new List<Student>();
        ContactTags = new List<ContactTag>();
    }

    public Contact(Guid personId, string phoneNumber) : this()
    {
        PersonId = personId;
        PhoneNumber = phoneNumber;
    }
}