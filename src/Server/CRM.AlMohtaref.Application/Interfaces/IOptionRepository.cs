using CRM.AlMohtaref.Domain.Entities.Questions;

namespace CRM.AlMohtaref.Application.Interfaces;

public interface IOptionRepository
{
    Task<List<Option>> GetByTextsWithTagsAsync(IReadOnlyCollection<string> optionTexts, CancellationToken cancellationToken = default);

    void Add(Option option);
}