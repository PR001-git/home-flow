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
    /// <summary>Returns all tasks, optionally filtered by assignee, status, or type.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? assignedToUserId,
        [FromQuery] HouseholdTaskStatus? status,
        [FromQuery] HouseholdTaskType? taskType,
        CancellationToken ct)
    {
        TaskFilterDto? filter = null;
        if (assignedToUserId.HasValue || status.HasValue || taskType.HasValue)
            filter = new TaskFilterDto(assignedToUserId, status, taskType);

        var result = await taskService.GetAllTasksAsync(filter, ct);
        return Ok(result);
    }

    /// <summary>Returns the task with the given ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await taskService.GetTaskByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>Creates a new household task assigned by the current user.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await taskService.CreateTaskAsync(request, userId, ct);
        return Created($"/api/tasks/{result.Id}", result);
    }

    /// <summary>Updates the mutable fields of the task with the given ID.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken ct)
    {
        var result = await taskService.UpdateTaskAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Permanently deletes the task with the given ID.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await taskService.DeleteTaskAsync(id, ct);
        return NoContent();
    }

    /// <summary>Marks the task with the given ID as completed by the current user.</summary>
    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await taskService.CompleteTaskAsync(id, userId, ct);
        return Ok(result);
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
