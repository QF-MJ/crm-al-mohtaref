using FluentValidation;

namespace CRM.AlMohtaref.Application.Common.Queries
{
    public class PagedQueryValidator : AbstractValidator<PagedQuery>
    {
        public PagedQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("يجب أن يكون رقم الصفحة أكبر من أو يساوي 1");
        }
    }
}
