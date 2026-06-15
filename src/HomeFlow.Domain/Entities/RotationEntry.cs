namespace HomeFlow.Domain.Entities;

public class RotationEntry
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid UserId { get; set; }
    public int RotationOrder { get; set; }
}
