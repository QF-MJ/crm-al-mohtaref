using CRM.AlMohtaref.Application.Common.Queries;
using CRM.AlMohtaref.Domain.Entities.Tasks;
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
            .WithMessage("نوع المهمة غير صالح")
            .NotEqual(TaskType.SubTask)
            .WithMessage("لا يمكن الفلترة بنوع مهمة فرعية")
            .When(x => x.Type.HasValue);

        RuleFor(x => x.StartTime)
         .NotNull()
         .WithMessage("تاريخ البداية مطلوب");

        RuleFor(x => x.EndTime)
            .NotNull()
            .WithMessage("تاريخ النهاية مطلوب");

        RuleFor(x => x.EndTime)
            .Must((x, end) => end!.Value.Date >= x.StartTime!.Value.Date)
            .When(x => x.StartTime.HasValue && x.EndTime.HasValue)
            .WithMessage("تاريخ النهاية يجب أن يكون أكبر من أو يساوي تاريخ البداية");

        RuleFor(x => x.AssignedToUserId )
            .NotEmpty()
            .WithMessage("معرف المنفذ مطلوب");

    }
}
