using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Users.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة")
            .MaximumLength(150).WithMessage("يجب ألا يتجاوز البريد الإلكتروني 150 حرفاً");

        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("اسم الحساب مطلوب")
            .MinimumLength(3).WithMessage("يجب أن يتكون اسم الحساب من 3 أحرف على الأقل")
            .MaximumLength(50).WithMessage("يجب ألا يتجاوز اسم الحساب 50 حرفاً")
            .Matches("^[a-zA-Z0-9._-]+$").WithMessage("اسم الحساب يجب أن يحتوي على أحرف إنجليزية وأرقام أو الرموز (. _ -) فقط دون مسافات");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("الاسم الكامل مطلوب")
            .MinimumLength(3).WithMessage("يجب أن يتكون الاسم الكامل من 3 أحرف على الأقل")
            .MaximumLength(100).WithMessage("يجب ألا يتجاوز الاسم الكامل 100 حرف");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة")
            .MinimumLength(8).WithMessage("يجب أن تتكون كلمة المرور من 8 خانات على الأقل")
            .MaximumLength(100).WithMessage("يجب ألا تتجاوز كلمة المرور 100 خانة")
            .Matches("[A-Z]").WithMessage("يجب أن تحتوي كلمة المرور على حرف كبير واحد على الأقل")
            .Matches("[a-z]").WithMessage("يجب أن تحتوي كلمة المرور على حرف صغير واحد على الأقل")
            .Matches("[0-9]").WithMessage("يجب أن تحتوي كلمة المرور على رقم واحد على الأقل")
            .Matches("[^a-zA-Z0-9]").WithMessage("يجب أن تحتوي كلمة المرور على رمز خاص واحد على الأقل (مثل @، #، $)");

        RuleForEach(x => x.Permissions)
            .SetValidator(new PermissionCommandValidator())
            .When(x => x.Permissions is not null && x.Permissions.Count > 0);
    }
}
