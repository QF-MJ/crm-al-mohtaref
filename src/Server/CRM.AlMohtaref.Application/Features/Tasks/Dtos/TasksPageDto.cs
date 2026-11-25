using CRM.AlMohtaref.Application.Common.Dtos;
using System.Text.Json.Serialization;

namespace CRM.AlMohtaref.Application.Features.Tasks.DTOs;

public sealed record TasksPageDto(
    PageDto Page,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    Dictionary<Guid, TaskGroupDto>? GroupTasks,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    List<TaskDto>? Tasks
);
