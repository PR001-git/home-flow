using HomeFlow.Domain.Enums;

namespace HomeFlow.Domain.Entities;

public class HouseholdTask
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public HouseholdTaskType TaskType { get; set; }
    public HouseholdTaskStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? TemplateId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
