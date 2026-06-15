namespace HomeFlow.Domain.Entities;

public class RecurringTaskTemplate
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int FrequencyDays { get; set; }
    public int CurrentAssigneeIndex { get; set; }
    public DateTime? LastGeneratedDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
