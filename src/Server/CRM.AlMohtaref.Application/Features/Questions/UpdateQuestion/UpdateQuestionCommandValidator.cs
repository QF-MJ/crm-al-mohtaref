using CRM.AlMohtaref.Domain.Entities.Questions;
using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Questions;

public class UpdateQuestionCommandValidator : AbstractValidator<UpdateQuestionCommand>
{
    public UpdateQuestionCommandValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty().WithMessage("معرف الحملة مطلوب");

        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("معرف السؤال مطلوب");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("نوع السؤال غير صالح");

        RuleFor(x => x.Target)
            .IsInEnum().WithMessage("الهدف غير صالح");

        When(x => x.Type == QuestionType.Text, () =>
        {
            RuleFor(x => x.FieldId)
                .NotEmpty().WithMessage("معرف الحقل مطلوب للسؤال النصي");

            RuleFor(x => x.Options)
                .Empty().WithMessage("السؤال النصي لا يجب أن يحتوي على خيارات");
        });

        When(x => x.Type != QuestionType.Text, () =>
        {
            RuleFor(x => x.FieldId)
                .Empty().WithMessage("معرف الحقل غير مسموح به لهذا النوع من الأسئلة");

            RuleFor(x => x.Options)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("يجب إضافة خيارات لهذا النوع من الأسئلة")
                .Must(HaveUniqueTexts).WithMessage("يوجد خيار مكرر في الطلب");

            RuleForEach(x => x.Options)
                .SetValidator(new QuestionOptionCommandValidator());
        });

        When(x => x.Type == QuestionType.Boolean, () =>
        {
            RuleFor(x => x.Options)
                .Must(options => options != null && options.Count >= 1 && options.Count <= 2)
                .WithMessage("السؤال المنطقي يجب أن يحتوي على خيارين على الأكثر وخيار على الأقل");
        });
    }

    private static bool HaveUniqueTexts(List<QuestionOptionCommand>? options)
    {
        return options is null
            || options
                .Select(o => (o.OptionText ?? string.Empty).Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() == options.Count;
    }
}