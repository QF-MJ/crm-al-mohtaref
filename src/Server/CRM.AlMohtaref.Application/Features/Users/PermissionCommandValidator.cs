using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Users
{
    public class PermissionCommandValidator : AbstractValidator<PermissionCommand>
    {
        public PermissionCommandValidator()
        {
            RuleFor(x => x.DepartmentId)
                .NotEmpty()
                .WithMessage("معرف القسم غير صالح");

            RuleFor(x => x.RoleId)
                .NotEmpty()
                .WithMessage("معرف الدور غير صالح");
        }
    }
}
