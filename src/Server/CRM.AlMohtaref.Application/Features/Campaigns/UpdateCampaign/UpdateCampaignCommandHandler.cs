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
            throw new NotFoundException($"الحملة بالمعرف {request.Id} غير موجودة");

        if (await campaignRepository.ExistsAsync(request.Id, request.Title, cancellationToken))
            throw new ConflictException("عنوان الحملة مستخدم مسبقاً");

        var targetPlatformIds = (request.PlatformIds ?? Enumerable.Empty<Guid>())
            .ToHashSet();

        var removedPlatformCampaignIds = campaign.PlatformCampaigns
            .Where(pc => !targetPlatformIds.Contains(pc.PlatformId))
            .Select(pc => pc.Id)
            .ToList();

        if (removedPlatformCampaignIds.Count > 0 &&  await campaignRepository.HasLeadsAsync(removedPlatformCampaignIds, cancellationToken))
        {
            throw new ConflictException("لا يمكن إزالة منصة تحتوي على عملاء محتملين مرتبطين بها");
        }

        campaign.UpdateDetails(
            request.Title,
            request.Description,
            request.StatusId,
            request.TrackId,
            request.Url,
            request.Budget
        );

        campaign.SynchronizePlatforms(targetPlatformIds);

        await unitOfWork.SaveChangesAsync();
    }
}
