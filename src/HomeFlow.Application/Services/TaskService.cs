using HomeFlow.Application.DTOs.Tasks;
using HomeFlow.Application.Exceptions;
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Enums;
using HomeFlow.Domain.Repositories;

namespace HomeFlow.Application.Services;

public class TaskService(ITaskRepository taskRepository, IUserRepository userRepository)
{
    public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 200)
            throw new ValidationException("Invalid title: must be between 1 and 200 characters.");

        if (request.DueDate.HasValue && request.DueDate.Value < DateTime.UtcNow)
            throw new ValidationException("Invalid due date: must be in the future.");

        if (request.AssignedToUserId.HasValue)
        {
            var assignedUser = await userRepository.GetByIdAsync(request.AssignedToUserId.Value);
            if (assignedUser is null)
                throw new ValidationException("Invalid assigned user: user not found.");
        }

        var task = new HouseholdTask
        {
            Title = request.Title,
            Description = request.Description,
            TaskType = HouseholdTaskType.OneOff,
            Status = HouseholdTaskStatus.Pending,
            DueDate = request.DueDate,
            AssignedToUserId = request.AssignedToUserId,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await taskRepository.CreateAsync(task);
        return MapToResponse(created);
    }

    public async Task<IEnumerable<TaskResponse>> GetAllTasksAsync(TaskFilterDto? filter)
    {
        TaskFilter? domainFilter = filter is not null
            ? new TaskFilter(filter.AssignedToUserId, filter.Status, filter.TaskType)
            : null;

        var tasks = await taskRepository.GetAllAsync(domainFilter);
        return tasks.Select(t => MapToResponse(FlagOverdue(t)));
    }

    public async Task<TaskResponse> GetTaskByIdAsync(Guid id)
    {
        var task = await taskRepository.GetByIdAsync(id);
        if (task is null)
            throw new NotFoundException($"Task with ID {id} not found.");

        return MapToResponse(FlagOverdue(task));
    }

    public async Task<TaskResponse> UpdateTaskAsync(Guid id, UpdateTaskRequest request)
    {
        var task = await taskRepository.GetByIdAsync(id);
        if (task is null)
            throw new NotFoundException($"Task with ID {id} not found.");

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 200)
            throw new ValidationException("Invalid title: must be between 1 and 200 characters.");

        if (request.AssignedToUserId.HasValue)
        {
            var user = await userRepository.GetByIdAsync(request.AssignedToUserId.Value);
            if (user is null)
                throw new ValidationException("Invalid assigned user: user not found.");
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.DueDate = request.DueDate;
        task.AssignedToUserId = request.AssignedToUserId;

        var updated = await taskRepository.UpdateAsync(task);
        return MapToResponse(updated);
    }

    public async Task<TaskResponse> CompleteTaskAsync(Guid id, Guid requestingUserId)
    {
        var task = await taskRepository.GetByIdAsync(id);
        if (task is null)
            throw new NotFoundException($"Task with ID {id} not found.");

        if (task.Status == HouseholdTaskStatus.Completed)
            throw new ValidationException("Task is already completed.");

        if (task.AssignedToUserId != requestingUserId && task.CreatedByUserId != requestingUserId)
            throw new ValidationException("You do not have permission to complete this task.");

        task.Status = HouseholdTaskStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;

        var updated = await taskRepository.UpdateAsync(task);
        return MapToResponse(updated);
    }

    public async System.Threading.Tasks.Task DeleteTaskAsync(Guid id)
    {
        var task = await taskRepository.GetByIdAsync(id);
        if (task is null)
            throw new NotFoundException($"Task with ID {id} not found.");

        await taskRepository.DeleteAsync(id);
    }

    private static HouseholdTask FlagOverdue(HouseholdTask task)
    {
        if (task.DueDate.HasValue
            && task.DueDate.Value < DateTime.UtcNow
            && task.Status is HouseholdTaskStatus.Pending or HouseholdTaskStatus.InProgress)
        {
            task.Status = HouseholdTaskStatus.Overdue;
        }
        return task;
    }

    private static TaskResponse MapToResponse(HouseholdTask task)
    {
        return new TaskResponse(
            task.Id, task.Title, task.Description, task.TaskType, task.Status,
            task.DueDate, task.AssignedToUserId, task.CreatedByUserId,
            task.TemplateId, task.CreatedAt, task.CompletedAt
        );
    }
}
