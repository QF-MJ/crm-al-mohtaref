using CRM.AlMohtaref.Domain.Entities.Questions;

namespace CRM.AlMohtaref.Application.Interfaces;

public interface IQuestionConfigurationRepository
{
    Task<Guid?> FindIdAsync(QuestionType type, QuestionTarget target, Guid? fieldId, CancellationToken cancellationToken = default);
}