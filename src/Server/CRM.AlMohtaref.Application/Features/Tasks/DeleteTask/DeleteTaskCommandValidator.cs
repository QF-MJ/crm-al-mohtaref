using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Tasks.DeleteTask;

public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
    public DeleteTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("معرف المهمة مطلوب");
    }
}
