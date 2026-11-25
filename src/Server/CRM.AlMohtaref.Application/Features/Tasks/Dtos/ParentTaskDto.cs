namespace CRM.AlMohtaref.Application.Features.Tasks.Dtos;

public sealed record ParentTaskDto(
    Guid Id,
    string Title,
    string TypeName
);
