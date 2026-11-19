using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Questions;
using CRM.AlMohtaref.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.AlMohtaref.Infrastructure.Repositories;

public class QuestionCampaignRepository(AppDbContext context) : IQuestionCampaignRepository
{
    public Task<QuestionCampaign?> GetByCampaignAndQuestionAsync(Guid campaignId, Guid questionId, CancellationToken cancellationToken = default)
    {
        return context.QuestionCampaigns
            .Include(x => x.Question)
            .FirstOrDefaultAsync(x => x.CampaignId == campaignId && x.QuestionId == questionId, cancellationToken);
    }

    public Task<bool> IsUsedByOtherCampaignsAsync(Guid campaignId, Guid questionId, CancellationToken cancellationToken = default)
    {
        return context.QuestionCampaigns
            .AsNoTracking()
            .AnyAsync(x => x.CampaignId != campaignId && x.QuestionId == questionId, cancellationToken);
    }
}