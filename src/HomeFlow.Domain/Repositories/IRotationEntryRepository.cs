using HomeFlow.Domain.Entities;

namespace HomeFlow.Domain.Repositories;

public interface IRotationEntryRepository
{
    /// <summary>Returns all rotation entries for the given template, ordered by rotation order.</summary>
    Task<IEnumerable<RotationEntry>> GetByTemplateIdAsync(Guid templateId, CancellationToken ct = default);
    /// <summary>Persists a new rotation entry and populates its generated ID.</summary>
    Task CreateAsync(RotationEntry entry, CancellationToken ct = default);
    /// <summary>Removes all rotation entries belonging to the given template.</summary>
    Task DeleteByTemplateIdAsync(Guid templateId, CancellationToken ct = default);
}
