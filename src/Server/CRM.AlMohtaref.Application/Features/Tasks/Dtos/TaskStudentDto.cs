using System.Text.Json.Serialization;

namespace CRM.AlMohtaref.Application.Features.Tasks.Dtos;

public sealed record TaskStudentDto(
    Guid StudentId,
    string StudentName,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    LeadStatusDto? LeadStatus = null
);
