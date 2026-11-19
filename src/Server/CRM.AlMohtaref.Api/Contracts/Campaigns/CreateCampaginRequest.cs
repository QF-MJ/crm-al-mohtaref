using CRM.AlMohtaref.Application.Features.Campaigns;

namespace CRM.AlMohtaref.Api.Contracts.Campaigns;

public record CreateCampaignRequest(string Title, string Description, Guid StatusId, Guid TrackId, string Url, decimal Budget, List<Guid> PlatformIds)
{
    public CreateCampaignCommand ToCommand() => new(
        Title,
        Description,
        TrackId,
        Url,
        Budget,
        PlatformIds
    );
}