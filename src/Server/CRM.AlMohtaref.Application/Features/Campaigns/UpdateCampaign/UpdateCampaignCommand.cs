using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Domain.Entities.Users;
using CRM.AlMohtaref.Domain.Enums;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Campaigns;

[AuthorizePermisions(Role = Role.Manager, Department = "التسويق")]
[AuthorizePermisions(Role = Role.Manager, Department = "الإدارة")]

public record UpdateCampaignCommand(
    Guid Id,
    string Title,
    string Description,
    CampaignStatus Status,
    Guid TrackId,
    string Url,
    decimal Budget,
    List<Guid> PlatformIds
) : IRequest;
