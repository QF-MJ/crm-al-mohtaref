using CRM.AlMohtaref.Domain.Entities.Questions;

namespace CRM.AlMohtaref.Application.Interfaces;

public interface IOptionSetRepository
{
    Task<Guid?> GetIdByHashAsync(string hash, CancellationToken cancellationToken = default);

    Task<bool> IsUsedByOtherAsync(Guid optionSetId, Guid questionCampaignId, CancellationToken cancellationToken);

    void Add(OptionSet optionSet);

    void Delete(OptionSet optionSet);
}