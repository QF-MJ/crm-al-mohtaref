namespace CRM.AlMohtaref.Application.Interfaces.Common;

public interface IGetDetailRepository<TDto, in TKey>
{
    Task<TDto?> GetDetailAsync(TKey id, CancellationToken ct = default);
}
