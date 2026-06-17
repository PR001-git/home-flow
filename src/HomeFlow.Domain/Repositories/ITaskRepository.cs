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
    /// <summary>Returns the task with the given ID, or <see langword="null"/> if not found.</summary>
    Task<HouseholdTask?> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>Returns all tasks matching the optional filter, ordered by creation date descending.</summary>
    Task<IEnumerable<HouseholdTask>> GetAllAsync(TaskFilter? filter, CancellationToken ct = default);
    /// <summary>Persists a new task and populates its generated ID.</summary>
    Task<HouseholdTask> CreateAsync(HouseholdTask task, CancellationToken ct = default);
    /// <summary>Updates the mutable fields of an existing task.</summary>
    Task<HouseholdTask> UpdateAsync(HouseholdTask task, CancellationToken ct = default);
    /// <summary>Permanently removes the task with the given ID.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
