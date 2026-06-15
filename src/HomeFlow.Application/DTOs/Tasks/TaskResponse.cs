using HomeFlow.Domain.Enums;

namespace HomeFlow.Application.DTOs.Tasks;

public record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    HouseholdTaskType TaskType,
    HouseholdTaskStatus Status,
    DateTime? DueDate,
    Guid? AssignedToUserId,
    Guid CreatedByUserId,
    Guid? TemplateId,
    DateTime CreatedAt,
    DateTime? CompletedAt
);
