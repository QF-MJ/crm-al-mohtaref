using CRM.AlMohtaref.Application.Features.Campaigns;

namespace CRM.AlMohtaref.Api.Contracts.Campaigns;

public record UpdateCampaignRequest(string Title, string Description, Guid StatusId, Guid TrackId, string Url, decimal Budget, List<Guid> PlatformIds)
{
    public UpdateCampaignCommand ToCommand(Guid id) => new(
        id,
        Title,
        Description,
        StatusId,
        TrackId,
        Url,
        Budget,
        PlatformIds
    );
}