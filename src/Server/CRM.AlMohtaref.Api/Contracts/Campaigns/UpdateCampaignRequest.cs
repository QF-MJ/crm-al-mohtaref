using CRM.AlMohtaref.Application.Features.Campaigns;
using CRM.AlMohtaref.Domain.Enums;

namespace CRM.AlMohtaref.Api.Contracts.Campaigns;

public record UpdateCampaignRequest(string Title, string Description, CampaignStatus Status, Guid TrackId, string Url, decimal Budget, List<Guid> PlatformIds)
{
    public UpdateCampaignCommand ToCommand(Guid id) => new(
        id,
        Title,
        Description,
        Status,
        TrackId,
        Url,
        Budget,
        PlatformIds
    );
}