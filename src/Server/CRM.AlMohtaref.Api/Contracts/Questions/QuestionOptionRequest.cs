using CRM.AlMohtaref.Application.Features.Questions;

namespace CRM.AlMohtaref.Api.Contracts.Questions;

public record QuestionOptionRequest(string OptionText, List<Guid> TagIds)
{
    public QuestionOptionCommand ToCommand() => new(OptionText, TagIds);
}