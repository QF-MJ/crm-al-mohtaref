using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Tasks.GetTask;

public sealed class GetTaskQueryValidator : AbstractValidator<GetTaskQuery>
{
    public GetTaskQueryValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("معرف المهمة مطلوب");
    }
}
