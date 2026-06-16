using FluentAssertions;
using HomeFlow.Application.Services;
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Enums;
using HomeFlow.Domain.Repositories;
using NSubstitute;

namespace HomeFlow.Application.Tests.Services;

public class DashboardServiceTests
{
    private readonly ITaskRepository _taskRepository = Substitute.For<ITaskRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly DashboardService _sut;

    public DashboardServiceTests()
    {
        _sut = new DashboardService(_taskRepository, _userRepository);
    }

    [Fact]
    public async Task GetDashboard_ComputesOverdueTodaysAndDistribution()
    {
        var pedro = new User { Id = Guid.NewGuid(), Username = "pedro", DisplayName = "Pedro" };
        var maria = new User { Id = Guid.NewGuid(), Username = "maria", DisplayName = "Maria" };
        _userRepository.GetAllAsync().Returns(new[] { pedro, maria });

        var overdue = new HouseholdTask { Id = Guid.NewGuid(), Title = "late", Status = HouseholdTaskStatus.Pending, DueDate = DateTime.UtcNow.AddDays(-1), AssignedToUserId = pedro.Id };
        var today = new HouseholdTask { Id = Guid.NewGuid(), Title = "today", Status = HouseholdTaskStatus.Pending, DueDate = DateTime.UtcNow.AddHours(1), AssignedToUserId = maria.Id };
        var done = new HouseholdTask { Id = Guid.NewGuid(), Title = "done", Status = HouseholdTaskStatus.Completed, AssignedToUserId = pedro.Id };
        _taskRepository.GetAllAsync(null).Returns(new[] { overdue, today, done });

        var result = await _sut.GetDashboardAsync();

        result.OverdueCount.Should().Be(1);
        result.TodaysTasks.Should().ContainSingle(t => t.Title == "today");
        result.TotalsByStatus.Completed.Should().Be(1);
        result.TotalsByStatus.Overdue.Should().Be(1);
        result.Distribution.Should().Contain(d => d.DisplayName == "Pedro" && d.ActiveCount == 1);
        result.Distribution.Should().Contain(d => d.DisplayName == "Maria" && d.ActiveCount == 1);
    }
}
