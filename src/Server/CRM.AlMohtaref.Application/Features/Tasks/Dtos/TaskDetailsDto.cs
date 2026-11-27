using System.Text.Json.Serialization;

namespace CRM.AlMohtaref.Application.Features.Tasks.Dtos;

public sealed record TaskDetailsDto(
    Guid Id,
    string Title,
    string? Description,
    string TypeName,
    string StatusName,
    DateTime StartTime,
    DateTime EndTime,
    TaskUserDto CreatedByUser,
    TaskUserDto AssignedToUser,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    ParentTaskDto? ParentTask = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<TaskSubTaskDto>? SubTasks = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<TaskContactDto>? Contacts = null
);
