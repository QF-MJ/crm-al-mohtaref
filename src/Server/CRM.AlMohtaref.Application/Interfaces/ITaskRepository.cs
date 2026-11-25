using CRM.AlMohtaref.Application.Features.Tasks.Dtos;
using CRM.AlMohtaref.Application.Features.Tasks.GetTasks;
using CRM.AlMohtaref.Application.Interfaces.Common;
using TaskEntity = CRM.AlMohtaref.Domain.Entities.Tasks.Task;

namespace CRM.AlMohtaref.Application.Interfaces;

public interface ITaskRepository :
    ISearchRepository<GetTasksQuery, TasksPageDto>,
    IGetByIdRepository<TaskEntity, Guid>,
    IDeleteRepository<TaskEntity>,
    IGetDetailRepository<TaskDetailsDto, Guid>
{

}
