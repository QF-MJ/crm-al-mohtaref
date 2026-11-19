using CRM.AlMohtaref.Application.Features.Questions;
using CRM.AlMohtaref.Domain.Entities.Questions;

namespace CRM.AlMohtaref.Api.Contracts.Questions;

public record UpdateQuestionRequest(QuestionType Type, QuestionTarget Target, Guid? FieldId, List<QuestionOptionRequest>? Options)
{
    public UpdateQuestionCommand ToCommand(Guid campaignId, Guid questionId) => new(
        campaignId,
        questionId,
        Type,
        Target,
        FieldId,
        Options?.Select(o => o.ToCommand()).ToList()
    );
}