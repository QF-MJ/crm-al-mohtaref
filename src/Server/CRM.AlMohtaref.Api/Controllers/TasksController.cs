using CRM.AlMohtaref.Api.Contracts.Tasks;
using CRM.AlMohtaref.Application.Features.Tasks.DeleteTask;
using CRM.AlMohtaref.Application.Features.Tasks.Dtos;
using CRM.AlMohtaref.Application.Features.Tasks.GetTask;
using CRM.AlMohtaref.Application.Features.Tasks.GetTasks;
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

        [HttpDelete("{taskId:guid}")]
        public async Task<IActionResult> DeleteTask(Guid taskId, CancellationToken ct)
        {
            await sender.Send(new DeleteTaskCommand(taskId), ct);
            return NoContent();
        }

        [HttpGet("{taskId:guid}")]
        public async Task<ActionResult<TaskDetailsDto>> GetTask(Guid taskId, CancellationToken ct)
        {
            return Ok(await sender.Send(new GetTaskQuery(taskId), ct));
        }
    }
}
