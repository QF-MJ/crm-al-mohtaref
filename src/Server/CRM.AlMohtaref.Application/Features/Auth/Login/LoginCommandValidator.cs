using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Auth.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(u => u.AccountName)
              .NotNull()
                .WithMessage("اسم الحساب أو كلمة المرور غير صحيحة")
              .NotEmpty()
                .WithMessage("اسم الحساب أو كلمة المرور غير صحيحة")
              .MaximumLength(50)
                .WithMessage("اسم الحساب أو كلمة المرور غير صحيحة");

            RuleFor(u => u.Password)
                .NotNull()
                    .WithMessage("اسم الحساب أو كلمة المرور غير صحيحة")
                .NotEmpty()
                    .WithMessage("اسم الحساب أو كلمة المرور غير صحيحة")
                .MaximumLength(100)
                    .WithMessage("اسم الحساب أو كلمة المرور غير صحيحة");
        }
    }
}
