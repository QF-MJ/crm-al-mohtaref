namespace CRM.AlMohtaref.Domain.Entities.Questions;


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
        Id = Guid.NewGuid();
        OptionText = optionText;
    }

    public void SyncTags(IEnumerable<Guid> tagIds)
    {
        var target = tagIds.ToHashSet();

        var toRemove = OptionTags.Where(t => !target.Contains(t.TagId)).ToList();
        foreach (var tag in toRemove)
            OptionTags.Remove(tag);

        var existing = OptionTags.Select(t => t.TagId).ToHashSet();
        foreach (var tagId in target.Where(id => !existing.Contains(id)))
            OptionTags.Add(new OptionTag(Id, tagId));
    }
}