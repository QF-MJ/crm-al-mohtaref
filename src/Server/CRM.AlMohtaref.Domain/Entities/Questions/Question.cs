namespace CRM.AlMohtaref.Domain.Entities.Questions;


public class Question
{
    public Guid Id { get; private set; }

    public string Text { get; private set; }

    public Guid ConfigurationId { get; private set; }

    public QuestionConfiguration Configuration { get; private set; }

    public ICollection<QuestionCampaign> QuestionCampaigns { get; private set; }

    private Question()
    {
        Text = string.Empty;
        Configuration = null!;
        QuestionCampaigns = new List<QuestionCampaign>();
    }

    public Question(string text, Guid configurationId) : this()
    {
        Text = text;
        ConfigurationId = configurationId;
    }
}
