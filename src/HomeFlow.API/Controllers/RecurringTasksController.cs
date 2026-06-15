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
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await recurringTaskService.GetAllTemplatesAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await recurringTaskService.GetTemplateByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecurringTaskRequest request)
    {
        var result = await recurringTaskService.CreateTemplateAsync(request);
        return Created($"/api/recurring-tasks/{result.Id}", result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRecurringTaskRequest request)
    {
        var result = await recurringTaskService.UpdateTemplateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await recurringTaskService.DeleteTemplateAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/generate")]
    public async Task<IActionResult> GenerateNext(Guid id)
    {
        var result = await recurringTaskService.GenerateNextTaskAsync(id);
        return Created($"/api/tasks/{result.Id}", result);
    }
}
