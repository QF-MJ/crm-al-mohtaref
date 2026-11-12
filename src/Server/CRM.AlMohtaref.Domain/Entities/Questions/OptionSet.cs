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
        Id = Guid.NewGuid();
        ContentHash = contentHash;
    }

    public static OptionSet Create(string contentHash, IEnumerable<Guid> optionIds)
    {
        var set = new OptionSet(contentHash);

        foreach (var optionId in optionIds)
            set.AddOption(optionId);

        return set;
    }

    public void AddOption(Guid optionId)
    {
        if (Items.Any(i => i.OptionId == optionId))
            return;

        Items.Add(new OptionSetItem(optionId));
    }
}