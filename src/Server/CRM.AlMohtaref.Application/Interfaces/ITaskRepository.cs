using CRM.AlMohtaref.Application.Features.Tasks.Dtos;
using CRM.AlMohtaref.Application.Features.Tasks.GetTasks;

namespace CRM.AlMohtaref.Application.Interfaces;

public interface ITaskRepository
{
    Task<TasksPageDto> SearchAsync(GetTasksQuery query, CancellationToken ct = default);
}