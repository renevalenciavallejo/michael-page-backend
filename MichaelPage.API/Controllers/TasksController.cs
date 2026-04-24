using MichaelPage.Application.Tasks;
using MichaelPage.Application.Tasks.Dto;
using Microsoft.AspNetCore.Mvc;
using TaskStatus = MichaelPage.Core.Enum.TaskStatus;

namespace MichaelPage.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class TasksController(ITaskService taskService, ILogger<TasksController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TaskStatus? status = null)
    {
        var result = await taskService.GetAllTasks(status);
        if (result.Success)
            return Ok(result);

        logger.LogError("TasksController - GetAll: {ErrorMessage}", result.Message);
        return BadRequest(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await taskService.GetTaskById(id);
        if (result.Success)
            return Ok(result);

        logger.LogWarning("TasksController - GetById: {ErrorMessage}", result.Message);
        return NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto input)
    {
        var result = await taskService.CreateTask(input);
        if (result.Success)
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);

        logger.LogError("TasksController - Create: {ErrorMessage}", result.Message);
        return BadRequest(result);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromQuery] TaskStatus status)
    {
        var result = await taskService.UpdateTaskStatus(id, status);
        if (result.Success)
            return Ok(result);

        logger.LogWarning("TasksController - UpdateStatus: {ErrorMessage}", result.Message);
        return BadRequest(result);
    }
}
