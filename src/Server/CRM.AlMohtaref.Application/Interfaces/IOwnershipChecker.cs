namespace CRM.AlMohtaref.Application.Interfaces;

public interface IOwnershipChecker<in T>
{
    Task<bool> IsOwnerAsync(T resource, CancellationToken ct);
}

