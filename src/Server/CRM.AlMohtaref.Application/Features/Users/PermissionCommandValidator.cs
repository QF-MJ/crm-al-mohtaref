using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Users
{
    public class PermissionCommandValidator : AbstractValidator<PermissionCommand>
    {
        public PermissionCommandValidator()
        {
            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .WithMessage("معرف القسم غير صحيح، يجب أن يكون أكبر من 0");

            RuleFor(x => x.RoleId)
                .GreaterThan(0)
                .WithMessage("معرف الدور غير صحيح، يجب أن يكون أكبر من 0");
        }
    }
}
