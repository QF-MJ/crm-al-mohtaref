using CRM.AlMohtaref.Application.Features.Tasks.UpdateTask;
using FluentValidation;

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("معرّف المهمة مطلوب");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان المهمة مطلوب")
            .MaximumLength(200).WithMessage("يجب ألا يتجاوز عنوان المهمة 200 حرف");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("يجب ألا يتجاوز وصف المهمة 2000 حرف")
            .When(x => x.Description is not null);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("حالة المهمة غير صالحة");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("وقت بدء المهمة مطلوب");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("وقت انتهاء المهمة مطلوب")
            .GreaterThanOrEqualTo(x => x.StartTime.Date).WithMessage("يجب أن يكون وقت الانتهاء مساوياً لتاريخ البدء أو بعده");

        RuleFor(x => x.AssignedToUserId)
            .NotEmpty().WithMessage("معرّف المستخدم المكلّف مطلوب");
    }
}
