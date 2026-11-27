using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Tasks.DeleteTask;

public class DeleteTaskCommandHandler(ITaskRepository taskRepository)
    : IRequestHandler<DeleteTaskCommand>
{
    public async Task Handle(DeleteTaskCommand command, CancellationToken ct)
    {
        if (!await taskRepository.ExistsAsync(command.TaskId, ct))
        {
            throw new NotFoundException($"المهمة بالمعرف \"{command.TaskId}\" غير موجودة");
        }

        if (await taskRepository.HasDependenciesAsync(command.TaskId, ct))
        {
            throw new ConflictException($"لا يمكن حذف المهمة بالمعرف \"{command.TaskId}\" لأنها مرتبطة بسجلات الطلاب أو بمهام فرعية");
        }

        await taskRepository.DeleteByIdAsync(command.TaskId, ct);
    }
}
