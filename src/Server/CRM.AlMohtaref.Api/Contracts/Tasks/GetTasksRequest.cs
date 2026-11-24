using CRM.AlMohtaref.Application.Features.Tasks.GetTasks;
using CRM.AlMohtaref.Domain.Entities.Tasks;

namespace CRM.AlMohtaref.Api.Contracts.Tasks;

public class GetTasksRequest : PagedRequest
{
    public string? Title { get; set; }

    public TaskType? Type { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public Guid AssignedToUserId  { get; set; }

    public bool GroupBySubTask { get; set; }

    public GetTasksQuery ToQuery() => new(
        AssignedToUserId ,
        PageNumber,
        Title,
        Type,
        StartTime,
        EndTime,
        GroupBySubTask
    );
}
