using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Questions;
using CRM.AlMohtaref.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.AlMohtaref.Infrastructure.Repositories;

public class QuestionConfigurationRepository(AppDbContext context) : IQuestionConfigurationRepository
{
    public Task<Guid?> FindIdAsync(QuestionType type, QuestionTarget target, Guid? fieldId, CancellationToken cancellationToken = default)
    {
        return context.QuestionConfigurations
            .AsNoTracking()
            .Where(c => c.Type == type && c.Target == target && c.FieldId == fieldId)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}