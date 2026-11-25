using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Features.Tasks.DTOs;
using CRM.AlMohtaref.Application.Features.Tasks.GetTasks;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Tasks;
using CRM.AlMohtaref.Domain.Extensions;
using CRM.AlMohtaref.Infrastructure.Data;
using CRM.AlMohtaref.Infrastructure.ExternalServices; 
using Microsoft.EntityFrameworkCore;


using TaskEntity = CRM.AlMohtaref.Domain.Entities.Tasks.Task;
namespace CRM.AlMohtaref.Infrastructure.Repositories;

public class TaskRepository(AppDbContext db)
    : ITaskRepository
{
    public void Delete(TaskEntity task, CancellationToken ct = default)
    {
        db.Tasks.Remove(task);
    }

    public async Task<TaskEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Tasks
                .Include(t => t.SubTasks)
                .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<TasksPageDto> SearchAsync(GetTasksQuery query, CancellationToken ct = default)
    {
        var baseQuery = db.Tasks
            .AsNoTracking()
            .Where(t => t.AssignedToUserId  == query.AssignedToUserId 
                && (string.IsNullOrWhiteSpace(query.Title) || t.Title.Contains(query.Title.Trim()))
                && (query.Type == null || t.TaskType == query.Type)
                && (query.StartTime == default || t.StartTime >= query.StartTime)
                && (query.EndTime == default || t.EndTime <= query.EndTime)
            );

        PageDto page = new PageDto(query.PageNumber, false);

        if (query.GroupBySubTask)
        {
            var rawSubTasks = await baseQuery
                .Where(t => t.TaskType == TaskType.SubTask && t.ParentTaskId != null)
                .OrderByDescending(t => t.EndTime)
                .Skip((query.PageNumber - 1) * PaginationSettings.PageSize)
                .Take(PaginationSettings.PageSize + 1)
                .Select(t => new
                {
                    ParentId = t.ParentTaskId!.Value,
                    ParentTitle = t.ParentTask!.Title,
                    SubTaskId = t.Id,
                    Status = t.TaskStatus,
                    Contacts = t.StudentTasks
                        .Select(st => new GroupContactDto(
                            st.Student.Contact.Id,
                            st.Student.Contact.FullName,
                            st.Student.Contact.PhoneNumber
                        ))
                        .Distinct()
                        .ToList()
                })
                .ToListAsync(ct);

            if (rawSubTasks.Count > PaginationSettings.PageSize)
            {
                page.HasNextPage = true;
                rawSubTasks.RemoveAt(rawSubTasks.Count - 1);
            }

            var groupTasks = rawSubTasks
                .GroupBy(x => new { x.ParentId, x.ParentTitle })
                .ToDictionary(
                    g => g.Key.ParentId,
                    g => new TaskGroupDto(
                        g.Key.ParentTitle,
                        g.Select(x => new GroupSubTaskDto(
                            x.SubTaskId,
                            x.Status.ToTaskStatusString(),
                            x.Contacts
                        )).ToList()
                    )
                );

            return new TasksPageDto(page, groupTasks, null);
        }

        var rawTasks = await baseQuery
            .Where(t => t.TaskType != TaskType.SubTask)
            .OrderByDescending(t => t.EndTime)
            .Skip((query.PageNumber - 1) * PaginationSettings.PageSize)
            .Take(PaginationSettings.PageSize + 1)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.TaskType,
                t.TaskStatus,
                t.EndTime,
                AssignedToUserFullName = t.AssignedToUser != null ? t.AssignedToUser.FullName : null
            })
            .ToListAsync(ct);

        if (rawTasks.Count > PaginationSettings.PageSize)
        {
            page.HasNextPage = true;
            rawTasks.RemoveAt(rawTasks.Count - 1);
        }

        var tasks = rawTasks
            .Select(t => new TaskDto(
                t.Id,
                t.Title,
                t.TaskType.ToTaskTypeString(),
                t.TaskStatus.ToTaskStatusString(),
                t.EndTime,
                t.AssignedToUserFullName
            ))
            .ToList();

        return new TasksPageDto(page, null, tasks);
    }
}
