using CRM.AlMohtaref.Domain.Entities.Campaigns;
using CRM.AlMohtaref.Domain.Entities.Contacts;
using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Domain.Entities.Tasks;

public class Task
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }


    
    public TaskType TaskType { get; set; }

    public TaskStatus TaskStatus { get; set; }

    
    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    
    public Guid? ParentTaskId { get; set; }

    public Task? ParentTask { get; set; }

    public ICollection<Task> SubTasks { get; set; } = new List<Task>();

    public Campaign? Campaign { get; set; }

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    
    public Guid AssignedToUserId  { get; set; }

    public User AssignedToUser { get; set; }

    
    public ICollection<StudentTask> StudentTasks { get; set; } = new List<StudentTask>();

    public void Update(string title, string? description, TaskStatus taskStatus, DateTime startTime, DateTime endTime, Guid assignedToUserId)
    {
        Title = title;
        Description = description;
        TaskStatus = taskStatus;
        StartTime = startTime;
        EndTime = endTime;
        AssignedToUserId = assignedToUserId;
    }


}
