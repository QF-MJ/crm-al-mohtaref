using CRM.AlMohtaref.Application.Features.Campaigns.Dtos;
using CRM.AlMohtaref.Application.Features.Campaigns.GetCampaignsPage;
using CRM.AlMohtaref.Domain.Entities.Campaigns;
using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Application.Interfaces;

public interface ICampaignRepository
{
    Task<Campaign?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<CampaignDto?> GetDtoByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsAsync(Guid id, string title, CancellationToken ct = default);

    Task<List<CampaignListItemDto>> GetAll(GetCampaignsPageQuery query);

    Task AddAsync(Campaign campaign);
}
