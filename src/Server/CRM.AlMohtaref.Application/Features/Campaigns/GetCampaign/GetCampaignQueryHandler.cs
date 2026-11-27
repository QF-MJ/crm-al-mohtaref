
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Campaigns.Dtos;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Campaigns.GetCampaign
{
    public class GetCampaignQueryHandler(ICampaignRepository _repo) : IRequestHandler<GetCampaignQuery, CampaignDto>
    {
        public async Task<CampaignDto> Handle(GetCampaignQuery query , CancellationToken ct)
        {
            CampaignDto? dto = await _repo.GetDtoByIdAsync(query.Id, ct);

            if(dto == null)
            {
                throw new NotFoundException("الحملة غير موجودة");
            }
            return dto;
        }
    }
}
