using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("يجب أن يكون معرف المستخدم أكبر من صفر");
    }
}
