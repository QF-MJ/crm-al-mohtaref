using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);

        RuleFor(x => x.Name).MaximumLength(200);
        RuleFor(x => x.AccountName).MaximumLength(100);
        RuleFor(x => x.Email).MaximumLength(256);

        RuleFor(x => x.PermissionId)
            .GreaterThan(0)
            .When(x => x.PermissionId.HasValue);
    }
}
