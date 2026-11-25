using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Features.Tasks.Dtos;
using CRM.AlMohtaref.Application.Features.Tasks.GetTasks;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Campaigns;
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
    // one student of a task, with his contact and his status in the campaign (only the columns the DTO needs)
    private sealed record StudentLink(
        Guid ContactId,
        string? ContactName,
        string PhoneNumber,
        Guid StudentId,
        string? StudentName,
        LeadStatus? Lead
    );

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

    public async Task<TaskDetailsDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var h = await db.Tasks
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                Task = x,
                CampaignId = (Guid?)x.Campaign!.Id
                             ?? (Guid?)x.ParentTask!.Campaign!.Id
                             ?? (Guid?)x.ParentTask!.ParentTask!.Campaign!.Id
            })
            .Select(y => new
            {
                y.Task.Title, y.Task.Description, y.Task.TaskType,
                y.Task.TaskStatus, y.Task.StartTime, y.Task.EndTime,
                CreatedById = y.Task.CreatedByUserId,
                CreatedByName = y.Task.CreatedByUser.FullName,
                y.Task.AssignedToUserId,
                AssignedToName = y.Task.AssignedToUser != null ? y.Task.AssignedToUser.FullName : null,
                y.Task.ParentTaskId,
                ParentTitle = y.Task.ParentTask != null ? y.Task.ParentTask.Title : null,
                ParentType = y.Task.ParentTask != null ? (TaskType?)y.Task.ParentTask.TaskType : null,

                Own = y.Task.StudentTasks
                    .Select(st => new StudentLink(
                        st.Student.Contact.Id,
                        st.Student.Contact.FullName,
                        st.Student.Contact.PhoneNumber,
                        st.Student.Id,
                        st.Student.FullName,
                        st.Student.CampaignLeads
                            .Where(l => l.CampaignId == y.CampaignId)
                            .Select(l => (LeadStatus?)l.Status)
                            .FirstOrDefault()))
                    .ToList(),

                Subs = db.Tasks
                    .Where(t => t.TaskType == TaskType.SubTask
                                && (t.ParentTaskId == y.Task.Id || t.ParentTask!.ParentTaskId == y.Task.Id))
                    .OrderBy(t => t.StartTime)
                    .Select(t => new
                    {
                        t.Id,
                        t.TaskStatus,
                        Links = t.StudentTasks
                            .Select(st => new StudentLink(
                                st.Student.Contact.Id,
                                st.Student.Contact.FullName,
                                st.Student.Contact.PhoneNumber,
                                st.Student.Id,
                                st.Student.FullName,
                                st.Student.CampaignLeads
                                    .Where(l => l.CampaignId == y.CampaignId)
                                    .Select(l => (LeadStatus?)l.Status)
                                    .FirstOrDefault()))
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (h == null)
            return null;

        var type = h.TaskType;
        var isIssue = (h.ParentType ?? type) == TaskType.Issue;

        // a contact (guardian) appears once per sub-task, with his students under him
        List<TaskContactDto> ToContacts(IEnumerable<StudentLink> links) => links
            .GroupBy(r => r.ContactId)
            .Select(g => new TaskContactDto(
                g.Key,
                g.First().ContactName!,
                g.First().PhoneNumber,
                isIssue ? null : g.Select(r => new TaskStudentDto(
                    r.StudentId,
                    r.StudentName!,
                    r.Lead == null ? null : new LeadStatusDto((int)r.Lead.Value, r.Lead.Value.ToLeadStatusString())
                )).ToList()))
            .ToList();

        // Normal: no sub-tasks, no contacts. SubTask: contacts only. Everything else: sub-tasks only.
        List<TaskSubTaskDto>? subTasks = type is TaskType.SubTask or TaskType.Normal
            ? null
            : h.Subs.Select(s => new TaskSubTaskDto(s.Id, s.TaskStatus.ToTaskStatusString(), ToContacts(s.Links))).ToList();

        List<TaskContactDto>? contacts = type == TaskType.SubTask
            ? ToContacts(h.Own)
            : null;

        return new TaskDetailsDto(
            id, h.Title,
            h.Description, type.ToTaskTypeString(),
            h.TaskStatus.ToTaskStatusString(),
            h.StartTime, h.EndTime,
            new TaskUserDto(h.CreatedById, h.CreatedByName!),
            h.AssignedToUserId == null ? null : new TaskUserDto(h.AssignedToUserId.Value, h.AssignedToName!),
            h.ParentTaskId == null ? null : new ParentTaskDto(h.ParentTaskId.Value, h.ParentTitle!, h.ParentType!.Value.ToTaskTypeString()),
            subTasks,
            contacts
        );
    }

    public async Task<TasksPageDto> SearchAsync(GetTasksQuery query, CancellationToken ct = default)
    {
        var baseQuery = db.Tasks
            .AsNoTracking()
            .Where(t => t.AssignedToUserId == query.AssignedToUserId
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