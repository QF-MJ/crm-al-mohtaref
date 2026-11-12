using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Questions;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Questions;

public class UpdateQuestionCommandHandler(IQuestionCampaignRepository questionCampaignRepository,IQuestionConfigurationRepository configurationRepository,
    IOptionRepository optionRepository, IOptionSetRepository optionSetRepository, IOptionSetHashService hashService,
    IUnitOfWork unitOfWork)
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
        {
            throw new ConflictException($"لا يمكن تغيير إعدادات سؤال مرتبط بحملات أخرى: {request.QuestionId}");
        }

        Guid? optionSetId = null;

        if (request.Type != QuestionType.Text)
            optionSetId = await ResolveOptionSetIdAsync(request.Options, cancellationToken);

        link.Update(configurationId, optionSetId);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Guid?> ResolveOptionSetIdAsync(List<QuestionOptionCommand>? options, CancellationToken cancellationToken)
    {
        if (options == null || options.Count == 0)
            return null;

        var requested = options
            .Select(o => (Text: o.OptionText.Trim(), o.TagIds))
            .ToList();

        var existing = (await optionRepository
                .GetByTextsWithTagsAsync(requested.Select(o => o.Text).ToList(), cancellationToken))
            .ToDictionary(o => o.OptionText, StringComparer.OrdinalIgnoreCase);

        var optionIds = new List<Guid>(requested.Count);
        var createdAny = false;

        foreach (var item in requested)
        {
            if (!existing.TryGetValue(item.Text, out var option))
            {
                option = new Option(item.Text);
                optionRepository.Add(option);
                createdAny = true;
            }

            option.SyncTags(item.TagIds);
            optionIds.Add(option.Id);
        }

        var hash = hashService.ComputeHash(optionIds);

        if (!createdAny)
        {
            var existingSetId = await optionSetRepository.GetIdByHashAsync(hash, cancellationToken);

            if (existingSetId is not null)
                return existingSetId;
        }

        var set = OptionSet.Create(hash, optionIds);
        optionSetRepository.Add(set);

        return set.Id;
    }
}