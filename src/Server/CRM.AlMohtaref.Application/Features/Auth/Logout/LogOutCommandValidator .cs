using CRM.AlMohtaref.Application.Common.Validators;
using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Auth.Logout
{
    public class LogoutCommandValidator : AbstractValidator<Logout.LogoutCommand>
    {
        public LogoutCommandValidator()
        {
            RuleFor(u => u.refreshToken)
                .Cascade(CascadeMode.Stop)
                .MustBeValidRefreshToken();
        }
    }
}