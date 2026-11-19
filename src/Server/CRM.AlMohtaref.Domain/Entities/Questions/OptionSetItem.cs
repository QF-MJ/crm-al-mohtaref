namespace CRM.AlMohtaref.Domain.Entities.Questions;

public class OptionSetItem
{
    public Guid Id { get; private set; }

    public Guid OptionSetId { get; private set; }

    public Guid OptionId { get; private set; }

    public OptionSet OptionSet { get; private set; }

    public Option Option { get; private set; }

    private OptionSetItem()
    {
        OptionSet = null!;
        Option = null!;
    }


    public OptionSetItem(Guid optionId) : this()
    {
        OptionId = optionId;
    }

    public OptionSetItem(Guid optionSetId, Guid optionId) : this()
    {
        OptionSetId = optionSetId;
        OptionId = optionId;
    }
}
