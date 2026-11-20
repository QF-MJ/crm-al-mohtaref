using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
         .NotEmpty()
         .WithMessage("معرف المستخدم غير صالح");

        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("اسم الحساب مطلوب")
            .MinimumLength(3).WithMessage("يجب أن يتكون اسم الحساب من 3 أحرف على الأقل")
            .MaximumLength(100).WithMessage("يجب ألا يتجاوز اسم الحساب 100 حرف")
            .Matches(@"\A[a-zA-Z0-9._-]+\z").WithMessage("اسم الحساب يجب أن يحتوي على أحرف إنجليزية وأرقام أو الرموز (. _ -) فقط دون مسافات");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("الاسم الكامل مطلوب")
            .MinimumLength(3).WithMessage("يجب أن يتكون الاسم الكامل من 3 أحرف على الأقل")
            .MaximumLength(100).WithMessage("يجب ألا يتجاوز الاسم الكامل 100 حرف");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة")
            .Matches(@"\A\S+\z").WithMessage("يجب ألا يحتوي البريد الإلكتروني على مسافات")
            .MaximumLength(256).WithMessage("يجب ألا يتجاوز البريد الإلكتروني 256 حرفاً");

        RuleFor(x => x.Permissions)
            .NotNull()
            .WithMessage("قائمة الصلاحيات مطلوبة ولا يمكن أن تكون فارغة")
            .NotEmpty()
            .WithMessage("يجب تحديد صلاحية واحدة على الأقل");

        RuleForEach(x => x.Permissions)
            .NotNull().WithMessage("عنصر الصلاحية لا يمكن أن يكون فارغاً")
            .SetValidator(new PermissionCommandValidator())
            .When(x => x.Permissions != null);
    }
}
