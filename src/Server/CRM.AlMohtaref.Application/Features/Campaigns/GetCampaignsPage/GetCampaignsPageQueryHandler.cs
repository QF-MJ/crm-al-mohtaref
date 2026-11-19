
using CRM.AlMohtaref.Application.Features.Campaigns.Dtos;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Campaigns.GetCampaignsPage
{
    public class GetCampaignsPageQueryHandler(ICampaignRepository _campaigns) : IRequestHandler<GetCampaignsPageQuery , List<CampaignListItemDto>>
    {
        public Task<List<CampaignListItemDto>> Handle(GetCampaignsPageQuery query , CancellationToken cancellationToken)
        {
            return _campaigns.GetAll(query);
        }

    }
}
