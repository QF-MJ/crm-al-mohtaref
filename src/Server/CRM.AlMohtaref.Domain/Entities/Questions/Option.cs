namespace CRM.AlMohtaref.Domain.Entities.Questions;

// An answer. Immutable by design: its text is never edited.
// To "change" an answer, add a different option to the option set and remove the old one.
public class Option
{
    public Guid Id { get; private set; }

    public string OptionText { get; private set; }

    public ICollection<OptionTag> OptionTags { get; private set; }

    public ICollection<OptionSetItem> OptionSetItems { get; private set; }

    private Option()
    {
        OptionText = string.Empty;
        OptionTags = new List<OptionTag>();
        OptionSetItems = new List<OptionSetItem>();
    }

    public Option(string optionText) : this()
    {
        OptionText = optionText;
    }
}
