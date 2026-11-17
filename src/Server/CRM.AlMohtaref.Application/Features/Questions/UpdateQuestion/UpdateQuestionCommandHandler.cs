using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Questions;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Questions;

public class UpdateQuestionCommandHandler(IQuestionCampaignRepository questionCampaignRepository, IQuestionConfigurationRepository configurationRepository,
    IOptionRepository optionRepository, IOptionSetRepository optionSetRepository, IOptionSetHashService hashService, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateQuestionCommand>
{
    public async Task Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
    {
        var link = await questionCampaignRepository
            .GetByCampaignAndQuestionAsync(request.CampaignId, request.QuestionId, cancellationToken)
            ?? throw new NotFoundException($"السؤال {request.QuestionId} غير مرتبط بالحملة {request.CampaignId}");

        var configurationId = await configurationRepository
            .FindIdAsync(request.Type, request.Target, request.FieldId, cancellationToken)
            ?? throw new NotFoundException($"لا توجد إعدادات مطابقة للسؤال {request.QuestionId}");

        if (link.IsConfigurationChanged(configurationId)
            && await questionCampaignRepository.IsUsedByOtherCampaignsAsync(request.CampaignId, request.QuestionId, cancellationToken))
              throw new ConflictException( $"لا يمكن تغيير إعدادات سؤال مرتبط بحملات أخرى: {request.QuestionId}");

        var oldOptionSet = link.OptionSet;

        var oldSetIsShared = oldOptionSet != null && await optionSetRepository
            .IsUsedByOtherAsync(oldOptionSet.Id, link.Id,  cancellationToken);

        Guid? newOptionSetId = null;

        if (request.Type != QuestionType.Text)
        {
            newOptionSetId = await ResolveOptionSetIdAsync(request.Options, link, oldSetIsShared, cancellationToken);
        }

        link.Update(configurationId, newOptionSetId);

        if (oldOptionSet != null && oldOptionSet.Id != newOptionSetId && !oldSetIsShared)
        {
            optionSetRepository.Delete(oldOptionSet);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Guid?> ResolveOptionSetIdAsync(List<QuestionOptionCommand>? options, QuestionCampaign link, bool oldSetIsShared, CancellationToken cancellationToken)
    {
        if (options == null || options.Count == 0)
            return null;

        var requested = options
            .Select(o => (Text: o.OptionText.Trim(),o.TagIds))
            .ToList();

        var existingOptions = await optionRepository
            .GetByTextsWithTagsAsync(requested.Select(o => o.Text).Distinct().ToList(), cancellationToken);

        var optionsByText = existingOptions.ToDictionary(o => o.OptionText, StringComparer.OrdinalIgnoreCase);

        var optionIds = new List<Guid>(requested.Count);

        foreach (var item in requested)
        {
            if (!optionsByText.TryGetValue(item.Text, out var option))
            {
                option = new Option(item.Text);

                optionRepository.Add(option);
                optionsByText.Add(item.Text, option);
            }

            option.SyncTags(item.TagIds);

            optionIds.Add(option.Id);
        }

        var hash = hashService.ComputeHash(optionIds);

        var existingSetId = await optionSetRepository
            .GetIdByHashAsync(hash, cancellationToken);

        if (existingSetId.HasValue)
            return existingSetId;

        if (link.OptionSet != null && !oldSetIsShared)
        {
            link.OptionSet.Update(hash, optionIds);
            return link.OptionSet.Id;
        }

        var newSet = OptionSet.Create(hash, optionIds);

        optionSetRepository.Add(newSet);

        return newSet.Id;
    }
}
