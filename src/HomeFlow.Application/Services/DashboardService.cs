using HomeFlow.Application.DTOs.Dashboard;
using HomeFlow.Application.DTOs.Tasks;
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Enums;
using HomeFlow.Domain.Repositories;

namespace HomeFlow.Application.Services;

public class DashboardService(ITaskRepository taskRepository, IUserRepository userRepository)
{
    public async Task<DashboardResponse> GetDashboardAsync()
    {
        var users = (await userRepository.GetAllAsync()).ToList();
        var tasks = (await taskRepository.GetAllAsync(null)).ToList();

        var effective = tasks.Select(t => (Task: t, Status: EffectiveStatus(t))).ToList();

        var today = DateTime.UtcNow.Date;
        var todaysTasks = effective
            .Where(e => e.Task.DueDate.HasValue && e.Task.DueDate.Value.Date == today)
            .Select(e => MapToResponse(e.Task, e.Status))
            .ToList();

        var overdueCount = effective.Count(e => e.Status == HouseholdTaskStatus.Overdue);

        var totals = new StatusTotals(
            effective.Count(e => e.Status == HouseholdTaskStatus.Pending),
            effective.Count(e => e.Status == HouseholdTaskStatus.InProgress),
            effective.Count(e => e.Status == HouseholdTaskStatus.Completed),
            effective.Count(e => e.Status == HouseholdTaskStatus.Overdue));

        var distribution = users.Select(u => new MemberDistribution(
            u.Id,
            u.DisplayName,
            effective.Count(e => e.Task.AssignedToUserId == u.Id && e.Status != HouseholdTaskStatus.Completed)));

        return new DashboardResponse(todaysTasks, overdueCount, totals, distribution);
    }

    private static HouseholdTaskStatus EffectiveStatus(HouseholdTask task)
    {
        if (task.DueDate.HasValue
            && task.DueDate.Value.Date < DateTime.UtcNow.Date
            && task.Status is HouseholdTaskStatus.Pending or HouseholdTaskStatus.InProgress)
        {
            return HouseholdTaskStatus.Overdue;
        }
        return task.Status;
    }

    private static TaskResponse MapToResponse(HouseholdTask task, HouseholdTaskStatus status) =>
        new(task.Id, task.Title, task.Description, task.TaskType, status,
            task.DueDate, task.AssignedToUserId, task.CreatedByUserId,
            task.TemplateId, task.CreatedAt, task.CompletedAt);
}
