using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("معرف المستخدم غير صالح");
    }
}
