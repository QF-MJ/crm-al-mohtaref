using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Campaigns;

[AuthorizePermisions(Role = Role.Manager, Department = "التسويق")]
[AuthorizePermisions(Role = Role.Manager , Department = "الإدارة")]

public record CreateCampaignCommand(
    string Title,
    string Description,
    Guid TrackId,
    string Url,
    decimal Budget,
    List<Guid> PlatformIds
) : IRequest<Guid>;
