using HomeFlow.Domain.Entities;

namespace HomeFlow.Domain.Repositories;

public interface IRotationEntryRepository
{
    Task<IEnumerable<RotationEntry>> GetByTemplateIdAsync(Guid templateId);
    Task CreateAsync(RotationEntry entry);
    Task DeleteByTemplateIdAsync(Guid templateId);
}
