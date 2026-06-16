using HomeFlow.Application.DTOs.RecurringTasks;
using HomeFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.API.Controllers;

[ApiController]
[Route("recurring-tasks")]
[Authorize]
public class RecurringTasksController(RecurringTaskService recurringTaskService) : ControllerBase
{
    /// <summary>Returns all recurring task templates with their rotation entries.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await recurringTaskService.GetAllTemplatesAsync(ct);
        return Ok(result);
    }

    /// <summary>Returns the recurring task template with the given ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await recurringTaskService.GetTemplateByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>Creates a new recurring task template with its rotation schedule.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecurringTaskRequest request, CancellationToken ct)
    {
        var result = await recurringTaskService.CreateTemplateAsync(request, ct);
        return Created($"/api/recurring-tasks/{result.Id}", result);
    }

    /// <summary>Updates the template with the given ID, replacing its rotation entries if a new list is provided.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRecurringTaskRequest request, CancellationToken ct)
    {
        var result = await recurringTaskService.UpdateTemplateAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Permanently deletes the recurring task template with the given ID.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await recurringTaskService.DeleteTemplateAsync(id, ct);
        return NoContent();
    }

    /// <summary>Generates the next task for the template using the current rotation slot and advances the assignee index.</summary>
    [HttpPost("{id:guid}/generate")]
    public async Task<IActionResult> GenerateNext(Guid id, CancellationToken ct)
    {
        var result = await recurringTaskService.GenerateNextTaskAsync(id, ct);
        return Created($"/api/tasks/{result.Id}", result);
    }
}
