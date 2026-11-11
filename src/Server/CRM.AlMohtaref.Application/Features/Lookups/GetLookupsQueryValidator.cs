using CRM.AlMohtaref.Application.Features.Lookups;
using FluentValidation;

public class GetLookupsQueryValidator : AbstractValidator<GetLookupsQuery>
{
    public GetLookupsQueryValidator()
    {
        RuleFor(x => x.Include)
            .NotEmpty()
            .WithMessage("يرجى تحديد البيانات المطلوب استرجاعها");

        RuleForEach(x => x.Include)
            .IsInEnum()
            .WithMessage("القيمة '{PropertyValue}' غير صالحة، يرجى اختيار خيار صحيح من القائمة المتاحة");
    }
}
