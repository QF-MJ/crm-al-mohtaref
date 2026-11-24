using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Lookups.GetUsers;

public sealed class GetUsersCommandValidator : AbstractValidator<GetUsersCommand>
{
    public GetUsersCommandValidator()
    {
        RuleFor(x => x.ManagerId)
            .NotEmpty()
            .WithMessage("معرّف المدير مطلوب.");
    }
}
