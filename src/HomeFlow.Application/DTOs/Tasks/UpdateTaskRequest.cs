namespace HomeFlow.Application.DTOs.Tasks;

public record UpdateTaskRequest(
    string Title,
    string? Description,
    DateTime? DueDate,
    Guid? AssignedToUserId
);
