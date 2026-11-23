using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Auth.Logout
{
    public class LogOutCommandValidator : AbstractValidator<LogOutCommand>
    {
        public LogOutCommandValidator()
        {
            RuleFor(u => u.refreshToken)
                .NotNull()
                    .WithMessage("رمز التحديث غير صالح")
                .NotEmpty()
                    .WithMessage("رمز التحديث غير صالح")
                .MaximumLength(500)
                    .WithMessage("رمز التحديث غير صالح")
                .Must(BeValidRefreshTokenFormat)
                    .WithMessage("رمز التحديث غير صالح");
        }

        private static bool BeValidRefreshTokenFormat(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var parts = refreshToken.Split('.');

            if (parts.Length != 2)
                return false;

            if (!Guid.TryParse(parts[0], out _))
                return false;

            return !string.IsNullOrWhiteSpace(parts[1]);
        }
    }
}