using CRM.AlMohtaref.Application.Common.Validators;
using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Auth.RefreshToken
{
    public class RefreshRefreshTokenCommandValidator : AbstractValidator<RefreshRefreshTokenCommand>
    {
        public RefreshRefreshTokenCommandValidator()
        {
            RuleFor(u => u.RefreshToken).MustBeValidRefreshToken();
        }
    }
}