using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Application.Interfaces.Common;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Campaigns;

public class UpdateCampaignStatusCommandHandler : IRequestHandler<UpdateCampaignStatusCommand>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCampaignStatusCommandHandler(ICampaignRepository campaignRepository, IUnitOfWork unitOfWork)
    {
        _campaignRepository = campaignRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCampaignStatusCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(request.Id, cancellationToken);

        if (campaign is null)
        {
            throw new NotFoundException("الحملة المطلوبة غير موجودة");
        }

        campaign.ChangeStatus(request.Status);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
