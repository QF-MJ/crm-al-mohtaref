namespace CRM.AlMohtaref.Application.Interfaces.Common;

public interface IOwnershipChecker<in T>
{
    Task<bool> IsOwnerAsync(T resource, CancellationToken ct);
}

