using CRM.AlMohtaref.Application.Interfaces.Common;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Options;

namespace CRM.AlMohtaref.Infrastructure.ExternalServices;

public class GoogleSheetsService : IGoogleSheetsService
{
    private readonly SheetsService _service;

    public GoogleSheetsService(IOptions<GoogleSheetsSettings> options)
    {
        var settings = options.Value;

        var path = Path.IsPathRooted(settings.CredentialsPath)
            ? settings.CredentialsPath
            : Path.Combine(AppContext.BaseDirectory, settings.CredentialsPath);

        var credential = GoogleCredential
            .FromFile(path)
            .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

        _service = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = settings.ApplicationName
        });
    }

    public async Task<IList<string>> GetQuestionsAsync(
        string spreadsheetId, string sheetName, CancellationToken ct = default)
    {
        // '...' حول اسم الورقة تحميك من المسافات والأحرف الخاصة
        var range = $"'{sheetName}'!1:1";

        var response = await _service.Spreadsheets.Values
            .Get(spreadsheetId, range)
            .ExecuteAsync(ct);

        return response.Values?.FirstOrDefault()?
            .Select(x => x?.ToString() ?? string.Empty)
            .ToList()
            ?? new List<string>();
    }
}