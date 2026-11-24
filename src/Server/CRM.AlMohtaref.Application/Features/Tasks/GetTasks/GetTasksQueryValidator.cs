using CRM.AlMohtaref.Application.Common.Queries;
using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Tasks.GetTasks;

public sealed class GetTasksQueryValidator : AbstractValidator<GetTasksQuery>
{
    public GetTasksQueryValidator()
    {
        Include(new PagedQueryValidator());

        RuleFor(x => x.Title)
            .MaximumLength(200)
            .WithMessage("عنوان المهمة يجب ألا يتجاوز 200 حرف")
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .When(x => x.Title is not null)
            .WithMessage("عنوان المهمة لا يمكن أن يكون فارغاً");

        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type.HasValue)
            .WithMessage("نوع المهمة غير صالح");

        RuleFor(x => x.EndTime)
            .GreaterThanOrEqualTo(x => x.StartTime)
            .WithMessage("تاريخ النهاية يجب أن يكون أكبر من أو يساوي تاريخ البداية")
            .When(x => x.StartTime != null);

        RuleFor(x => x.AssignedToUserId )
            .NotEmpty()
            .WithMessage("معرف المنفذ مطلوب");

    }
}
