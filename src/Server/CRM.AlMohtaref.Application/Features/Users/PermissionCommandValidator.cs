using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.AlMohtaref.Application.Features.Users
{
    public class PermissionCommandValidator : AbstractValidator<PermissionCommand>
    {
        public PermissionCommandValidator()
        {
            RuleFor(x => x.DepartmentId).GreaterThan(0);

            RuleFor(x => x.RoleId).GreaterThan(0);
        }
    }
}
