namespace CRM.AlMohtaref.Application.Interfaces.Common;

public interface IGetByIdRepository<TEntity, in TKey> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);
}
