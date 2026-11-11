namespace CRM.AlMohtaref.Domain.Entities.Questions;


public class OptionSet
{
    public Guid Id { get; private set; }

    public string ContentHash { get; private set; }

    public ICollection<OptionSetItem> Items { get; private set; }

    public ICollection<QuestionCampaign> QuestionCampaigns { get; private set; }

    private OptionSet()
    {
        ContentHash = string.Empty;
        Items = new List<OptionSetItem>();
        QuestionCampaigns = new List<QuestionCampaign>();
    }

    public OptionSet(string contentHash) : this()
    {
        ContentHash = contentHash;
    }

    public void ReHash(string hash)
    {
        ContentHash = hash;
    }

    
    public void AddOption(Guid optionId)
    {
        if (Items.Any(i => i.OptionId == optionId))
            return;

        Items.Add(new OptionSetItem(optionId));
    }

    public void RemoveOption(Guid optionId)
    {
        var item = Items.FirstOrDefault(i => i.OptionId == optionId);

        if (item is not null)
            Items.Remove(item);
    }

    public OptionSet CloneAs(string hash)
    {
        var clone = new OptionSet(hash);

        foreach (var item in Items)
            clone.Items.Add(new OptionSetItem(item.OptionId));

        return clone;
    }
}
