using HomeFlow.Domain.Entities;

namespace HomeFlow.Domain.Repositories;

public interface IRecurringTaskTemplateRepository
{
    /// <summary>Returns the template with the given ID, or <see langword="null"/> if not found.</summary>
    Task<RecurringTaskTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>Returns all recurring task templates ordered by creation date descending.</summary>
    Task<IEnumerable<RecurringTaskTemplate>> GetAllAsync(CancellationToken ct = default);
    /// <summary>Persists a new template and populates its generated ID.</summary>
    Task<RecurringTaskTemplate> CreateAsync(RecurringTaskTemplate template, CancellationToken ct = default);
    /// <summary>Updates the mutable fields of an existing template.</summary>
    Task<RecurringTaskTemplate> UpdateAsync(RecurringTaskTemplate template, CancellationToken ct = default);
    /// <summary>Permanently removes the template with the given ID.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
