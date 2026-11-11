using CRM.AlMohtaref.Domain.Entities.Contacts;

namespace CRM.AlMohtaref.Domain.Entities.Questions;

public class OptionTag
{
    public Guid Id { get; private set; }

    public Guid OptionId { get; private set; }

    public Guid TagId { get; private set; }

    public Option Option { get; private set; }

    public Tag Tag { get; private set; }

    private OptionTag()
    {
        Option = null!;
        Tag = null!;
    }

    public OptionTag(Guid optionId, Guid tagId) : this()
    {
        OptionId = optionId;
        TagId = tagId;
    }
}
