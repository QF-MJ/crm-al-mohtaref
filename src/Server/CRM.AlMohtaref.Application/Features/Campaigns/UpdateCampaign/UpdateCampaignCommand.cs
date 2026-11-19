using CRM.AlMohtaref.Application.Common.Attributes;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Campaigns;

[AuthorizePermisions(Role = "مدير", Department = "التسويق")]
[AuthorizePermisions(Role = "مدير", Department = "الإدارة")]

public record UpdateCampaignCommand(
    Guid Id,
    string Title,
    string Description,
    Guid StatusId,
    Guid TrackId,
    string Url,
    decimal Budget,
    List<Guid> PlatformIds
) : IRequest;
