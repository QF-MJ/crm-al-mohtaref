using CRM.AlMohtaref.Application.Features.Tasks.DTOs;
using CRM.AlMohtaref.Application.Features.Tasks.GetTasks;

using Task = CRM.AlMohtaref.Domain.Entities.Tasks.Task;
namespace CRM.AlMohtaref.Application.Interfaces;

public interface ITaskRepository
{
    Task<TasksPageDto> SearchAsync(GetTasksQuery query, CancellationToken ct = default);

    Task<Task?> GetByIdAsync(Guid id, CancellationToken ct = default);

    void Delete(Task task, CancellationToken ct = default);
}