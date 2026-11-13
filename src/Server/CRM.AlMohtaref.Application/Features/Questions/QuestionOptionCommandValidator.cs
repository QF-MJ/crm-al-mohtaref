using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Questions;

public class QuestionOptionCommandValidator : AbstractValidator<QuestionOptionCommand>
{
    public QuestionOptionCommandValidator()
    {
        RuleFor(x => x.OptionText)
            .NotEmpty().WithMessage("نص الخيار مطلوب");

        RuleFor(x => x.TagIds)
            .NotEmpty().WithMessage("يجب تحديد وسم (Tag) واحد على الأقل لكل خيار");

        RuleForEach(x => x.TagIds)
            .NotEmpty().WithMessage("معرف الوسم غير صالح");
    }
}