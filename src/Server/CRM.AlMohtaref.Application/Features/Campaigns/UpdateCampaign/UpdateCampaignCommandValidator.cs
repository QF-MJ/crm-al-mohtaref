using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Campaigns;

public class UpdateCampaignCommandValidator : AbstractValidator<UpdateCampaignCommand>
{
    public UpdateCampaignCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("معرف الحملة مطلوب");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان الحملة مطلوب")
            .MaximumLength(200).WithMessage("يجب ألا يتجاوز العنوان 200 حرف");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("يجب ألا يتجاوز الوصف 1000 حرف");

        RuleFor(x => x.StatusId)
            .NotEmpty().WithMessage("حالة الحملة مطلوبة");

        RuleFor(x => x.TrackId)
            .NotEmpty().WithMessage("المسار مطلوب");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("رابط الحملة مطلوب")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("صيغة الرابط غير صحيحة");

        RuleFor(x => x.Budget)
            .GreaterThanOrEqualTo(0).WithMessage("يجب أن تكون الميزانية صفراً أو أكثر");

        RuleFor(x => x.PlatformIds)
            .NotEmpty().WithMessage("يجب اختيار منصة واحدة على الأقل");
    }
}
