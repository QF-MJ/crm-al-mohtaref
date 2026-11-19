using CRM.AlMohtaref.Domain.Entities.Questions;

namespace CRM.AlMohtaref.Application.Interfaces;

public interface IOptionSetRepository
{
    Task<Guid?> GetIdByHashAsync(string hash, CancellationToken cancellationToken = default);

    void Add(OptionSet optionSet);
}