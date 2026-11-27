using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.AlMohtaref.Api.Controllers
{
    [Route("api/google/sheets")]
    [ApiController]
    public class GoogleSheetsController(ISender sender) : ControllerBase
    {

        [HttpGet("questions/{spreadsheetId}/{sheetName}")]
        public async Task<IActionResult> GetSheetQuestions(
          string spreadsheetId, string sheetName)
        {
            var questions = await sender.Send(new GetSheetQuestionsQuery(spreadsheetId, sheetName));
            return Ok(questions);
        }
    }
}
