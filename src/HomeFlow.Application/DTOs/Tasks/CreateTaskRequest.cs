namespace HomeFlow.Application.DTOs.Tasks;

public record CreateTaskRequest(
    string Title,
    string? Description,
    DateTime? DueDate,
    Guid? AssignedToUserId
);
