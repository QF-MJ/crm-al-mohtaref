namespace CRM.AlMohtaref.Application.Interfaces.Common;

public interface IExistsRepository<TEntity, TKey> where TEntity : class
{
    Task<bool> ExistsAsync(TKey id, CancellationToken ct = default);
}
