using CRM.AlMohtaref.Domain.Entities.Questions;

namespace CRM.AlMohtaref.Domain.Entities.Contacts;

public class Tag
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public ICollection<ContactTag> ContactTags { get; private set; }

    public ICollection<OptionTag> OptionTags { get; private set; }

    private Tag()
    {
        Name = string.Empty;
        ContactTags = new List<ContactTag>();
        OptionTags = new List<OptionTag>();
    }

    public Tag(string name) : this()
    {
        Name = name;
    }
}
