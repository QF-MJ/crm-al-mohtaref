
using CRM.AlMohtaref.Application.Features.Campaigns.Dtos;
using CRM.AlMohtaref.Application.Features.Users;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Campaigns.GetCampaign;
public sealed record GetCampaignQuery(Guid Id) : IRequest<CampaignDto>;

