using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Contacts;

public sealed class GetContactQueryValidator : AbstractValidator<GetContactQuery>
{
    public GetContactQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("معرف جهة التواصل غير صالح");
    }
}
