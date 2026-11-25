namespace CRM.AlMohtaref.Application.Interfaces.Common;

public interface ISearchRepository<in TQuery, TResult>
{
    Task<TResult> SearchAsync(TQuery query, CancellationToken ct = default);
}
