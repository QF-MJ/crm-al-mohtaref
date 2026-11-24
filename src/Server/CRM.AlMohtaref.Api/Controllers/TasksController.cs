using CRM.AlMohtaref.Api.Contracts.Tasks;
using CRM.AlMohtaref.Application.Features.Tasks;
using CRM.AlMohtaref.Application.Features.Tasks.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.AlMohtaref.Api.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    [Authorize]
    public class TasksController(ISender sender)
        : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<TasksPageDto>> GetTasks([FromQuery] GetTasksRequest request, CancellationToken ct)
        {
            return Ok(await sender.Send(request.ToQuery(), ct));
        }
    }
}
