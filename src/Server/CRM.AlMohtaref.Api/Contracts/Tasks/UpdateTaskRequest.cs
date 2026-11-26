using CRM.AlMohtaref.Application.Features.Tasks.UpdateTask;
using TaskStatus = CRM.AlMohtaref.Domain.Entities.Tasks.TaskStatus;

namespace CRM.AlMohtaref.Api.Contracts.Tasks;

public class UpdateTaskRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskStatus Status { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public Guid AssignedToUserId { get; set; }


    public UpdateTaskCommand ToCommand(Guid taskId) => new(
        taskId,
        Title,
        Description,
        Status,
        StartTime,
        EndTime,
        AssignedToUserId
    );
}
