namespace CRM.AlMohtaref.Domain.Entities.Contacts;

public class Tag
{
    public int Id { get; private set; }

    public string Name { get; private set; }

    public ICollection<ContactTag> ContactTags { get; private set; }

    private Tag()
    {
        Name = string.Empty;
        ContactTags = new List<ContactTag>();
    }

    public Tag(string name) : this()
    {
        Name = name;
    }
}