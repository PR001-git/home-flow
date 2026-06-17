using HomeFlow.Application.DTOs.Tasks;

namespace HomeFlow.Application.DTOs.Dashboard;

public record StatusTotals(int Pending, int InProgress, int Completed, int Overdue);

public record MemberDistribution(Guid UserId, string DisplayName, int ActiveCount);

public record DashboardResponse(
    IEnumerable<TaskResponse> TodaysTasks,
    int OverdueCount,
    StatusTotals TotalsByStatus,
    IEnumerable<MemberDistribution> Distribution
);
