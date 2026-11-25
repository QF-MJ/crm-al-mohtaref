using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Campaigns;

public class UpdateCampaignStatusCommandValidator : AbstractValidator<UpdateCampaignStatusCommand>
{
    public UpdateCampaignStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("معرف الحملة مطلوب");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("حالة الحملة غير صالحة");
    }
}
