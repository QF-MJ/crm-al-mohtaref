using MediatR;
using CRM.AlMohtaref.Application.Interfaces.Common;

public record GetSheetQuestionsQuery(string SpreadsheetId, string SheetName) : IRequest<IList<string>>;

public class GetSheetQuestionsQueryHandler : IRequestHandler<GetSheetQuestionsQuery, IList<string>>
{
    private readonly IGoogleSheetsService _sheets;

    public GetSheetQuestionsQueryHandler(IGoogleSheetsService sheets) => _sheets = sheets;

    public Task<IList<string>> Handle(GetSheetQuestionsQuery request, CancellationToken ct)
        => _sheets.GetQuestionsAsync(request.SpreadsheetId, request.SheetName, ct);
}
