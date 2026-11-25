namespace CRM.AlMohtaref.Application.Interfaces.Common
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
