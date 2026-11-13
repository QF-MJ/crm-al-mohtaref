using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Campaigns;

public class UpdateCampaignCommandHandler(ICampaignRepository campaignRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCampaignCommand>
{
    public async Task Handle(UpdateCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await campaignRepository.GetByIdAsync(request.Id);

        if (campaign == null)
            throw new NotFoundException($"الحملة بالمعرف {request.Id} غير موجودة ");

        if(await campaignRepository.ExistsAsync(request.Id, request.Title, cancellationToken))
            throw new ConflictException("عنوان الحملة مستخدم مسبقاً");

        campaign.UpdateDetails
        (
            request.Title,
            request.Description,
            request.StatusId,
            request.TrackId,
            request.Url,
            request.Budget
        );

        campaign.SynchronizePlatforms
        (
            request.PlatformIds ?? new List<Guid>()
        );

        await unitOfWork.SaveChangesAsync();
    }
}
