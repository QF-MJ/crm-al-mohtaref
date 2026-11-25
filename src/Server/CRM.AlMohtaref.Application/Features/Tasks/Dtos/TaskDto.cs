namespace CRM.AlMohtaref.Application.Features.Tasks.DTOs;

public sealed record TaskDto(
    Guid Id,
    string Title,
    string TypeName,
    string StatusName,
    DateTime EndTime,
    string? AssignedToUserFullName
);
