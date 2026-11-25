using CRM.AlMohtaref.Application.Common.Queries;
using CRM.AlMohtaref.Application.Features.Tasks.DTOs;
using CRM.AlMohtaref.Domain.Entities.Tasks;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Tasks.GetTasks;

public sealed record GetTasksQuery(
    Guid AssignedToUserId ,
    int PageNumber = 1,
    string? Title = null,
    TaskType? Type = null,
    DateTime? StartTime = default,
    DateTime? EndTime = default,
    bool GroupBySubTask = false
) : PagedQuery(PageNumber), IRequest<TasksPageDto>;
