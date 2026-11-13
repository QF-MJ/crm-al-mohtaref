using CRM.AlMohtaref.Domain.Entities.Campaigns;

namespace CRM.AlMohtaref.Application.Interfaces;

public interface ICampaignRepository
{
    Task<Campaign?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsAsync(Guid id, string title, CancellationToken ct = default);

    Task<bool> HasLeadsAsync(IEnumerable<Guid> platformCampaignIds, CancellationToken cancellationToken);
}
