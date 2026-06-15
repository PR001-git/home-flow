using HomeFlow.Domain.Enums;

namespace HomeFlow.Application.DTOs.Tasks;

public record TaskFilterDto(
    Guid? AssignedToUserId,
    HouseholdTaskStatus? Status,
    HouseholdTaskType? TaskType
);
