namespace CRM.AlMohtaref.Application.Interfaces.Common;

public interface IDeleteRepository<in TEntity, in TKey> where TEntity : class
{
    Task DeleteByIdAsync(TKey id, CancellationToken ct = default);
}
