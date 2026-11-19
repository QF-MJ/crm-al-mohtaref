using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Domain.Entities.Questions;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Questions;


[AuthorizePermisions(Role = "مدير", Department = "التسويق")]
[AuthorizePermisions(Role = "مدير", Department = "الإدارة")]

public record UpdateQuestionCommand(
    Guid CampaignId,
    Guid QuestionId,
    QuestionType Type,
    QuestionTarget Target,
    Guid? FieldId,
    List<QuestionOptionCommand>? Options
) : IRequest;