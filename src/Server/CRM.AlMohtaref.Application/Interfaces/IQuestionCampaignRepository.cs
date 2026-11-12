using CRM.AlMohtaref.Domain.Entities.Questions;

namespace CRM.AlMohtaref.Application.Interfaces;

public interface IQuestionCampaignRepository
{
    Task<QuestionCampaign?> GetByCampaignAndQuestionAsync(Guid campaignId, Guid questionId, CancellationToken cancellationToken = default);

    Task<bool> IsUsedByOtherCampaignsAsync(Guid campaignId, Guid questionId, CancellationToken cancellationToken = default);
}