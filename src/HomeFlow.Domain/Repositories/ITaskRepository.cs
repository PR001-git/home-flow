using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Enums;

namespace HomeFlow.Domain.Repositories;

public record TaskFilter(
    Guid? AssignedToUserId,
    HouseholdTaskStatus? Status,
    HouseholdTaskType? TaskType
);

public interface ITaskRepository
{
    Task<HouseholdTask?> GetByIdAsync(Guid id);
    Task<IEnumerable<HouseholdTask>> GetAllAsync(TaskFilter? filter);
    Task<HouseholdTask> CreateAsync(HouseholdTask task);
    Task<HouseholdTask> UpdateAsync(HouseholdTask task);
    Task DeleteAsync(Guid id);
}
