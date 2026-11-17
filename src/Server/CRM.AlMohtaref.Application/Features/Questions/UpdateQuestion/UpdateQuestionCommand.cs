using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Domain.Entities.Questions;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Questions;


[AuthorizePermisions(Role = Role.Manager, Department = "التسويق")]
[AuthorizePermisions(Role = Role.Manager, Department = "الإدارة")]

public record UpdateQuestionCommand(
    Guid CampaignId,
    Guid QuestionId,
    QuestionType Type,
    QuestionTarget Target,
    Guid? FieldId,
    List<QuestionOptionCommand>? Options
) : IRequest;