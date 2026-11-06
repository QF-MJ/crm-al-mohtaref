using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("معرف المستخدم غير صحيح");

        RuleFor(x => x.AccountName)
            .NotEmpty()
            .WithMessage("اسم الحساب مطلوب")
            .MaximumLength(100)
            .WithMessage("يجب ألا يتجاوز اسم الحساب 100 حرف");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("الاسم الكامل مطلوب")
            .MaximumLength(200)
            .WithMessage("يجب ألا يتجاوز الاسم الكامل 200 حرف");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("البريد الإلكتروني مطلوب")
            .EmailAddress()
            .WithMessage("صيغة البريد الإلكتروني غير صحيحة")
            .MaximumLength(256)
            .WithMessage("يجب ألا يتجاوز البريد الإلكتروني 256 حرف");

        RuleFor(x => x.Permissions)
            .NotNull()
            .WithMessage("قائمة الصلاحيات مطلوبة ولا يمكن أن تكون فارغة")
            .NotEmpty()
            .WithMessage("يجب تحديد صلاحية واحدة على الأقل");

        RuleForEach(x => x.Permissions)
            .SetValidator(new PermissionCommandValidator())
            .When(x => x.Permissions != null);
    }
}
