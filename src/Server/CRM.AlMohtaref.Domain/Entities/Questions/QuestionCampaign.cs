using CRM.AlMohtaref.Domain.Entities.Campaigns;

namespace CRM.AlMohtaref.Domain.Entities.Questions;

public class QuestionCampaign
{
    public Guid Id { get; private set; }

    public Guid QuestionId { get; private set; }

    public Guid CampaignId { get; private set; }

    // null for free-text questions
    public Guid? OptionSetId { get; private set; }

    public Question Question { get; private set; }

    public Campaign Campaign { get; private set; }

    public OptionSet? OptionSet { get; private set; }

    private QuestionCampaign()
    {
        Question = null!;
        Campaign = null!;
    }

    public QuestionCampaign(Guid questionId, Guid campaignId, Guid? optionSetId = null) : this()
    {
        QuestionId = questionId;
        CampaignId = campaignId;
        OptionSetId = optionSetId;
    }

    public void ChangeOptionSet(Guid? optionSetId)
    {
        OptionSetId = optionSetId;
    }
}
