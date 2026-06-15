using FluentAssertions;
using HomeFlow.Application.DTOs.Tasks;
using HomeFlow.Application.Exceptions;
using HomeFlow.Application.Services;
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Enums;
using HomeFlow.Domain.Repositories;
using NSubstitute;

namespace HomeFlow.Application.Tests.Services;

public class TaskServiceTests
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly TaskService _sut;

    public TaskServiceTests()
    {
        _taskRepository = Substitute.For<ITaskRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _sut = new TaskService(_taskRepository, _userRepository);
    }

    [Fact]
    public async Task CreateTask_ValidInput_ReturnsTaskResponse()
    {
        var createdBy = Guid.NewGuid();
        var assignedTo = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(1);
        var request = new CreateTaskRequest("Buy groceries", "Weekly shopping", dueDate, assignedTo);

        _userRepository.GetByIdAsync(assignedTo).Returns(new User { Id = assignedTo });
        _taskRepository.CreateAsync(Arg.Any<HouseholdTask>()).Returns(callInfo =>
        {
            var t = callInfo.Arg<HouseholdTask>();
            t.Id = Guid.NewGuid();
            return t;
        });

        var result = await _sut.CreateTaskAsync(request, createdBy);

        result.Title.Should().Be("Buy groceries");
        result.TaskType.Should().Be(HouseholdTaskType.OneOff);
        result.Status.Should().Be(HouseholdTaskStatus.Pending);
        result.AssignedToUserId.Should().Be(assignedTo);
    }

    [Fact]
    public async Task GetAllTasks_OverdueTasks_StatusSetToOverdue()
    {
        var tasks = new List<HouseholdTask>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Overdue task",
                Status = HouseholdTaskStatus.Pending,
                DueDate = DateTime.UtcNow.AddDays(-1),
                CreatedByUserId = Guid.NewGuid()
            }
        };
        _taskRepository.GetAllAsync(Arg.Any<TaskFilter?>()).Returns(tasks);

        var result = (await _sut.GetAllTasksAsync(null)).ToList();

        result[0].Status.Should().Be(HouseholdTaskStatus.Overdue);
    }

    [Fact]
    public async Task CompleteTask_ValidRequest_SetsCompletedStatus()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = new HouseholdTask
        {
            Id = taskId,
            Title = "Test",
            Status = HouseholdTaskStatus.Pending,
            AssignedToUserId = userId,
            CreatedByUserId = userId
        };
        _taskRepository.GetByIdAsync(taskId).Returns(task);
        _taskRepository.UpdateAsync(Arg.Any<HouseholdTask>()).Returns(callInfo => callInfo.Arg<HouseholdTask>());

        var result = await _sut.CompleteTaskAsync(taskId, userId);

        result.Status.Should().Be(HouseholdTaskStatus.Completed);
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteTask_AlreadyCompleted_ThrowsValidationException()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = new HouseholdTask
        {
            Id = taskId,
            Title = "Test",
            Status = HouseholdTaskStatus.Completed,
            AssignedToUserId = userId,
            CreatedByUserId = userId
        };
        _taskRepository.GetByIdAsync(taskId).Returns(task);

        var act = () => _sut.CompleteTaskAsync(taskId, userId);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*already completed*");
    }

    [Fact]
    public async Task CompleteTask_NotAssignedOrCreator_ThrowsValidationException()
    {
        var taskId = Guid.NewGuid();
        var task = new HouseholdTask
        {
            Id = taskId,
            Title = "Test",
            Status = HouseholdTaskStatus.Pending,
            AssignedToUserId = Guid.NewGuid(),
            CreatedByUserId = Guid.NewGuid()
        };
        _taskRepository.GetByIdAsync(taskId).Returns(task);

        var act = () => _sut.CompleteTaskAsync(taskId, Guid.NewGuid());

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*permission*");
    }
}
