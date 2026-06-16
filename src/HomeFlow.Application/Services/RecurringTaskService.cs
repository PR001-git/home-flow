using HomeFlow.Application.DTOs.RecurringTasks;
using HomeFlow.Application.DTOs.Tasks;
using HomeFlow.Application.Exceptions;
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Enums;
using HomeFlow.Domain.Repositories;

namespace HomeFlow.Application.Services;

public class RecurringTaskService(
    IRecurringTaskTemplateRepository templateRepository,
    IRotationEntryRepository rotationEntryRepository,
    ITaskRepository taskRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<RecurringTaskResponse> CreateTemplateAsync(CreateRecurringTaskRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 200)
            throw new ValidationException("Invalid title: must be between 1 and 200 characters.");

        if (request.FrequencyDays < 1)
            throw new ValidationException("Invalid frequency: must be at least 1 day.");

        if (request.UserIdsInOrder is null || request.UserIdsInOrder.Count == 0)
            throw new ValidationException("Invalid rotation: must include at least one user.");

        foreach (var userId in request.UserIdsInOrder)
        {
            var user = await userRepository.GetByIdAsync(userId, ct);
            if (user is null)
                throw new ValidationException($"Invalid user: user with ID {userId} not found.");
        }

        var template = new RecurringTaskTemplate
        {
            Title = request.Title,
            Description = request.Description,
            FrequencyDays = request.FrequencyDays,
            CurrentAssigneeIndex = 0,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var created = await templateRepository.CreateAsync(template, ct);

            for (var i = 0; i < request.UserIdsInOrder.Count; i++)
            {
                await rotationEntryRepository.CreateAsync(new RotationEntry
                {
                    TemplateId = created.Id,
                    UserId = request.UserIdsInOrder[i],
                    RotationOrder = i
                }, ct);
            }

            await unitOfWork.CommitAsync(ct);

            var entries = await rotationEntryRepository.GetByTemplateIdAsync(created.Id, ct);
            return MapToResponse(created, entries);
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IEnumerable<RecurringTaskResponse>> GetAllTemplatesAsync(CancellationToken ct = default)
    {
        var templates = await templateRepository.GetAllAsync(ct);
        var results = new List<RecurringTaskResponse>();

        foreach (var template in templates)
        {
            var entries = await rotationEntryRepository.GetByTemplateIdAsync(template.Id, ct);
            results.Add(MapToResponse(template, entries));
        }

        return results;
    }

    public async Task<RecurringTaskResponse> GetTemplateByIdAsync(Guid id, CancellationToken ct = default)
    {
        var template = await templateRepository.GetByIdAsync(id, ct);
        if (template is null)
            throw new NotFoundException($"Template with ID {id} not found.");

        var entries = await rotationEntryRepository.GetByTemplateIdAsync(id, ct);
        return MapToResponse(template, entries);
    }

    public async Task<RecurringTaskResponse> UpdateTemplateAsync(Guid id, UpdateRecurringTaskRequest request, CancellationToken ct = default)
    {
        var template = await templateRepository.GetByIdAsync(id, ct);
        if (template is null)
            throw new NotFoundException($"Template with ID {id} not found.");

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 200)
            throw new ValidationException("Invalid title: must be between 1 and 200 characters.");

        if (request.FrequencyDays < 1)
            throw new ValidationException("Invalid frequency: must be at least 1 day.");

        if (request.UserIdsInOrder is not null && request.UserIdsInOrder.Count > 0)
        {
            foreach (var userId in request.UserIdsInOrder)
            {
                var user = await userRepository.GetByIdAsync(userId, ct);
                if (user is null)
                    throw new ValidationException($"Invalid user: user with ID {userId} not found.");
            }
        }

        template.Title = request.Title;
        template.Description = request.Description;
        template.FrequencyDays = request.FrequencyDays;

        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var updated = await templateRepository.UpdateAsync(template, ct);

            if (request.UserIdsInOrder is not null && request.UserIdsInOrder.Count > 0)
            {
                await rotationEntryRepository.DeleteByTemplateIdAsync(id, ct);
                for (var i = 0; i < request.UserIdsInOrder.Count; i++)
                {
                    await rotationEntryRepository.CreateAsync(new RotationEntry
                    {
                        TemplateId = id,
                        UserId = request.UserIdsInOrder[i],
                        RotationOrder = i
                    }, ct);
                }

                updated.CurrentAssigneeIndex = 0;
                updated = await templateRepository.UpdateAsync(updated, ct);
            }

            await unitOfWork.CommitAsync(ct);

            var entries = await rotationEntryRepository.GetByTemplateIdAsync(id, ct);
            return MapToResponse(updated, entries);
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct);
            throw;
        }
    }

    public async System.Threading.Tasks.Task DeleteTemplateAsync(Guid id, CancellationToken ct = default)
    {
        var template = await templateRepository.GetByIdAsync(id, ct);
        if (template is null)
            throw new NotFoundException($"Template with ID {id} not found.");

        await templateRepository.DeleteAsync(id, ct);
    }

    public async Task<TaskResponse> GenerateNextTaskAsync(Guid templateId, CancellationToken ct = default)
    {
        var template = await templateRepository.GetByIdAsync(templateId, ct);
        if (template is null)
            throw new NotFoundException($"Template with ID {templateId} not found.");

        var entries = (await rotationEntryRepository.GetByTemplateIdAsync(templateId, ct))
            .OrderBy(e => e.RotationOrder)
            .ToList();

        if (entries.Count == 0)
            throw new ValidationException("Cannot generate a task: the template has no rotation members.");

        var currentEntry = entries[template.CurrentAssigneeIndex % entries.Count];

        var task = new HouseholdTask
        {
            Title = template.Title,
            Description = template.Description,
            TaskType = HouseholdTaskType.Recurring,
            Status = HouseholdTaskStatus.Pending,
            DueDate = DateTime.UtcNow.AddDays(template.FrequencyDays),
            AssignedToUserId = currentEntry.UserId,
            CreatedByUserId = currentEntry.UserId,
            TemplateId = templateId,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.BeginTransactionAsync(ct);
        HouseholdTask createdTask;
        try
        {
            createdTask = await taskRepository.CreateAsync(task, ct);

            template.CurrentAssigneeIndex = (template.CurrentAssigneeIndex + 1) % entries.Count;
            template.LastGeneratedDate = DateTime.UtcNow;
            await templateRepository.UpdateAsync(template, ct);

            await unitOfWork.CommitAsync(ct);
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct);
            throw;
        }

        return new TaskResponse(
            createdTask.Id, createdTask.Title, createdTask.Description,
            createdTask.TaskType, createdTask.Status, createdTask.DueDate,
            createdTask.AssignedToUserId, createdTask.CreatedByUserId,
            createdTask.TemplateId, createdTask.CreatedAt, createdTask.CompletedAt
        );
    }

    private static RecurringTaskResponse MapToResponse(RecurringTaskTemplate template, IEnumerable<RotationEntry> entries)
    {
        return new RecurringTaskResponse(
            template.Id, template.Title, template.Description,
            template.FrequencyDays, template.CurrentAssigneeIndex,
            template.LastGeneratedDate, template.CreatedAt,
            entries.OrderBy(e => e.RotationOrder)
                .Select(e => new RotationEntryResponse(e.UserId, e.RotationOrder))
                .ToList()
        );
    }
}
