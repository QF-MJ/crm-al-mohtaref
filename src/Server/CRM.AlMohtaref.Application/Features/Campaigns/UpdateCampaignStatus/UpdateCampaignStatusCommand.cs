using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Domain.Entities.Campaigns;
using CRM.AlMohtaref.Domain.Enums;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Campaigns;

[AuthorizePermisions(Role = "مدير", Department = "التسويق")]
[AuthorizePermisions(Role = "مدير", Department = "الإدارة")]

public record UpdateCampaignStatusCommand(
    Guid Id,
    CampaignStatus Status
) : IRequest;
