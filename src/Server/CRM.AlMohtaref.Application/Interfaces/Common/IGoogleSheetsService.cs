
namespace CRM.AlMohtaref.Application.Interfaces.Common
{
    public interface IGoogleSheetsService
    {
         Task<IList<string>> GetQuestionsAsync(string spreadsheetId, string sheetName, CancellationToken ct = default);
    }
}
