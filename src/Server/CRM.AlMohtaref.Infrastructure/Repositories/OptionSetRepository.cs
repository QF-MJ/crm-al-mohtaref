using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Questions;
using CRM.AlMohtaref.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.AlMohtaref.Infrastructure.Repositories;

public class OptionSetRepository(AppDbContext context) : IOptionSetRepository
{
    public Task<Guid?> GetIdByHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        return context.OptionSets
            .AsNoTracking()
            .Where(s => s.ContentHash == hash)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(OptionSet optionSet)
    {
        context.OptionSets.Add(optionSet);
    }

    public async Task<bool> IsUsedByOtherAsync(Guid optionSetId, Guid questionCampaignId, CancellationToken cancellationToken)
    {
        return await context.QuestionCampaigns
            .AnyAsync(x => x.OptionSetId == optionSetId && x.Id != questionCampaignId, cancellationToken);
    }

    public void Delete(OptionSet optionSet)
    {
        context.OptionSets.Remove(optionSet);
    }
}