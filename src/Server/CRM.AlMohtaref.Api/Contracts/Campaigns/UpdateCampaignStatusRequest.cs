using CRM.AlMohtaref.Application.Features.Campaigns;
using CRM.AlMohtaref.Domain.Enums;

namespace CRM.AlMohtaref.Api.Contracts.Campaigns;

public record UpdateCampaignStatusRequest(CampaignStatus Status)
{
    public UpdateCampaignStatusCommand ToCommand(Guid id) => new(
        id,
        Status
    );
}
