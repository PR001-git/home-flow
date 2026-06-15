namespace HomeFlow.Application.DTOs.RecurringTasks;

public record CreateRecurringTaskRequest(
    string Title,
    string? Description,
    int FrequencyDays,
    List<Guid> UserIdsInOrder
);
