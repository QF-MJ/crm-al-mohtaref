using System.Text.Json.Serialization;

namespace CRM.AlMohtaref.Application.Features.Tasks.Dtos;

public sealed record TaskContactDto(
    Guid Id,
    string FullName,
    string PhoneNumber,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<TaskStudentDto>? Students = null
);
