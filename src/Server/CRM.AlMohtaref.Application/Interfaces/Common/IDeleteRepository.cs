namespace CRM.AlMohtaref.Application.Interfaces.Common;

public interface IDeleteRepository<in TEntity> where TEntity : class
{
    void Delete(TEntity entity, CancellationToken ct = default);
}
