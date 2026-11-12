using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Questions;
using CRM.AlMohtaref.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.AlMohtaref.Infrastructure.Repositories;

public class OptionRepository(AppDbContext context) 
    : IOptionRepository
{
    public Task<List<Option>> GetByTextsWithTagsAsync(IReadOnlyCollection<string> optionTexts, CancellationToken cancellationToken = default)
    {
        return context.Options
            .Include(o => o.OptionTags)
            .Where(o => optionTexts.Contains(o.OptionText))
            .ToListAsync(cancellationToken);
    }

    public void Add(Option option)
    {
        context.Options.Add(option);
    }
}