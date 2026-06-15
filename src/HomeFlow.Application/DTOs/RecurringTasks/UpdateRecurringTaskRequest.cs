namespace HomeFlow.Application.DTOs.RecurringTasks;

public record UpdateRecurringTaskRequest(
    string Title,
    string? Description,
    int FrequencyDays,
    List<Guid>? UserIdsInOrder
);
