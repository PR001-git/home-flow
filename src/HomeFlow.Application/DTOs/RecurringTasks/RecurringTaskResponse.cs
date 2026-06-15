namespace HomeFlow.Application.DTOs.RecurringTasks;

public record RotationEntryResponse(Guid UserId, int RotationOrder);

public record RecurringTaskResponse(
    Guid Id,
    string Title,
    string? Description,
    int FrequencyDays,
    int CurrentAssigneeIndex,
    DateTime? LastGeneratedDate,
    DateTime CreatedAt,
    List<RotationEntryResponse> RotationEntries
);
