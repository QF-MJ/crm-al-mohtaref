using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Notes.CreateNote;

public class CreateNoteCommandValidator : AbstractValidator<CreateNoteCommand>
{
    public CreateNoteCommandValidator()
    {
        RuleFor(x => x.ContactId)
            .NotEmpty().WithMessage("معرف جهة الاتصال مطلوب.");

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("نص الملاحظة لا يمكن أن يكون فارغاً.")
            .MaximumLength(2000).WithMessage("نص الملاحظة يجب ألا يتجاوز 2000 حرف.");
    }
}
