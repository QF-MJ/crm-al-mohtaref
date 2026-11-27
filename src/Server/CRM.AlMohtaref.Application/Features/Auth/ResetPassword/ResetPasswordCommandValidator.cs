using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Auth.ResetPassword
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.userId)
                .NotEmpty().WithMessage("معرف المستخدم مطلوب");

            RuleFor(x => x.newPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("كلمة المرور الجديدة مطلوبة")
                .MinimumLength(8).WithMessage("يجب ألا تقل كلمة المرور عن 8 أحرف")
                .MaximumLength(100).WithMessage("يجب ألا تتجاوز كلمة المرور 100 حرف");

            RuleFor(x => x.confirmPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("تأكيد كلمة المرور مطلوب")
                .Equal(x => x.newPassword).WithMessage("كلمتا المرور غير متطابقتين");
        }
    }
}
