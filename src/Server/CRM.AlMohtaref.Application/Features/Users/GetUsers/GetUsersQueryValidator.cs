using FluentValidation;
using CRM.AlMohtaref.Application.Common.Queries;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        Include(new PagedQueryValidator());

        RuleFor(x => x.Name)
            .MaximumLength(200)
            .WithMessage("يجب ألا يتجاوز الاسم 200 حرف");

        RuleFor(x => x.AccountName)
            .MaximumLength(100)
            .WithMessage("يجب ألا يتجاوز اسم الحساب 100 حرف");

        RuleFor(x => x.Email)
            .MaximumLength(256)
            .WithMessage("يجب ألا يتجاوز البريد الإلكتروني 256 حرف");

        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("معرف الدور غير صالح")
            .When(x => x.RoleId.HasValue);

        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithMessage("معرف القسم غير صالح")
            .When(x => x.DepartmentId.HasValue);
    }
}
