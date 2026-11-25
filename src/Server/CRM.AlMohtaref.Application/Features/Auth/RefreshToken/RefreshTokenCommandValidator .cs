using CRM.AlMohtaref.Application.Common.Validators;
using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Auth.RefreshToken
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(u => u.RefreshToken)
                .Cascade(CascadeMode.Stop)
                .MustBeValidRefreshToken();
        }
    }
}