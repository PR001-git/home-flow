using HomeFlow.Domain.Entities;

namespace HomeFlow.Domain.Repositories;

public interface IRecurringTaskTemplateRepository
{
    Task<RecurringTaskTemplate?> GetByIdAsync(Guid id);
    Task<IEnumerable<RecurringTaskTemplate>> GetAllAsync();
    Task<RecurringTaskTemplate> CreateAsync(RecurringTaskTemplate template);
    Task<RecurringTaskTemplate> UpdateAsync(RecurringTaskTemplate template);
    Task DeleteAsync(Guid id);
}
