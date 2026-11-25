using FluentValidation;

namespace CRM.AlMohtaref.Application.Common.Validators
{
    public static class RefreshTokenRuleExtensions
    {
        public static IRuleBuilderOptions<T, string?> MustBeValidRefreshToken<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("رمز التحديث غير صالح")
                .MaximumLength(500).WithMessage("رمز التحديث غير صالح")
                .Must(token =>
                {
                    if (token is null) return true; 
                    var parts = token.Split('.');
                    return parts.Length == 2
                        && Guid.TryParse(parts[0], out _)
                        && !string.IsNullOrWhiteSpace(parts[1]);
                }).WithMessage("رمز التحديث غير صالح");
        }
    }
}