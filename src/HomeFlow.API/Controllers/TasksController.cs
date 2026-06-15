using System.Security.Claims;
using HomeFlow.Application.DTOs.Tasks;
using HomeFlow.Application.Services;
using HomeFlow.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class TasksController(TaskService taskService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? assignedToUserId,
        [FromQuery] HouseholdTaskStatus? status,
        [FromQuery] HouseholdTaskType? taskType)
    {
        TaskFilterDto? filter = null;
        if (assignedToUserId.HasValue || status.HasValue || taskType.HasValue)
            filter = new TaskFilterDto(assignedToUserId, status, taskType);

        var result = await taskService.GetAllTasksAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await taskService.GetTaskByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await taskService.CreateTaskAsync(request, userId);
        return Created($"/api/tasks/{result.Id}", result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request)
    {
        var result = await taskService.UpdateTaskAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await taskService.DeleteTaskAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await taskService.CompleteTaskAsync(id, userId);
        return Ok(result);
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
