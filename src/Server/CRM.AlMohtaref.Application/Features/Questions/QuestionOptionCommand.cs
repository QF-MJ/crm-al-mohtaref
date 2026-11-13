namespace CRM.AlMohtaref.Application.Features.Questions;

public record QuestionOptionCommand(
     string OptionText,
     List<Guid> TagIds
);