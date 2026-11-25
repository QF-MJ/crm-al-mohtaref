using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Application.Interfaces.Common;
using CRM.AlMohtaref.Domain.Entities.Campaigns;
using CRM.AlMohtaref.Domain.Enums;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Campaigns;

public class CreateCampaignCommandHandler(ICampaignRepository campaignRepository, 
    IUnitOfWork unitOfWork ,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateCampaignCommand , Guid>
{

    public async Task<Guid> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {

        if (await campaignRepository.ExistsAsync(Guid.NewGuid() , request.Title, cancellationToken))
            throw new ConflictException("عنوان الحملة مستخدم مسبقاً");

        Campaign campaign = new Campaign(Guid.NewGuid(), request.Title, request.TrackId, currentUser!.UserId!.Value, CampaignStatus.New , request.Description, request.Url, request.Budget);

        //campaign.SynchronizePlatforms
        //(
        //    request.PlatformIds ?? new List<Guid>()
        //);

        //campaign.SynchronizeQuastions();

        await campaignRepository.AddAsync(campaign);
        await unitOfWork.SaveChangesAsync();

        return campaign.Id;
    }
}
