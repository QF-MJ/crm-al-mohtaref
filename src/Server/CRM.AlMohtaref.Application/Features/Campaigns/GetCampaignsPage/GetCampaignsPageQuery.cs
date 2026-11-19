

using CRM.AlMohtaref.Application.Common.Queries;
using CRM.AlMohtaref.Application.Features.Campaigns.Dtos;
using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Domain.Enums;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Campaigns.GetCampaignsPage;

public sealed record GetCampaignsPageQuery(
    int PageNumber = 1,
    string? Title = null,
    decimal? Budget = null,
    string? TrackName = null,
    CampaignStatus? CampaignStatus = null) 
    : PagedQuery(PageNumber), IRequest<CampaignListItemDto>;

