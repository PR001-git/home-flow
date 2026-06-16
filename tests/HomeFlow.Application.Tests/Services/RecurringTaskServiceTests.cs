using FluentAssertions;
using HomeFlow.Application.DTOs.RecurringTasks;
using HomeFlow.Application.Exceptions;
using HomeFlow.Application.Services;
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Enums;
using HomeFlow.Domain.Repositories;
using NSubstitute;

namespace HomeFlow.Application.Tests.Services;

public class RecurringTaskServiceTests
{
    private readonly IRecurringTaskTemplateRepository _templateRepository;
    private readonly IRotationEntryRepository _rotationEntryRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RecurringTaskService _sut;

    public RecurringTaskServiceTests()
    {
        _templateRepository = Substitute.For<IRecurringTaskTemplateRepository>();
        _rotationEntryRepository = Substitute.For<IRotationEntryRepository>();
        _taskRepository = Substitute.For<ITaskRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new RecurringTaskService(_templateRepository, _rotationEntryRepository, _taskRepository, _userRepository, _unitOfWork);
    }

    [Fact]
    public async Task CreateTemplate_ValidInput_CreatesTemplateAndRotationEntries()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var request = new CreateRecurringTaskRequest("Clean kitchen", "Deep clean", 7, new List<Guid> { user1, user2 });

        _userRepository.GetByIdAsync(user1).Returns(new User { Id = user1 });
        _userRepository.GetByIdAsync(user2).Returns(new User { Id = user2 });
        _templateRepository.CreateAsync(Arg.Any<RecurringTaskTemplate>()).Returns(callInfo =>
        {
            var t = callInfo.Arg<RecurringTaskTemplate>();
            t.Id = Guid.NewGuid();
            return t;
        });
        _rotationEntryRepository.GetByTemplateIdAsync(Arg.Any<Guid>()).Returns(new List<RotationEntry>
        {
            new() { UserId = user1, RotationOrder = 0 },
            new() { UserId = user2, RotationOrder = 1 }
        });

        var result = await _sut.CreateTemplateAsync(request);

        result.Title.Should().Be("Clean kitchen");
        result.FrequencyDays.Should().Be(7);
        result.RotationEntries.Should().HaveCount(2);
        await _rotationEntryRepository.Received(2).CreateAsync(Arg.Any<RotationEntry>());
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _unitOfWork.Received(1).CommitAsync();
        await _unitOfWork.DidNotReceive().RollbackAsync();
    }

    [Fact]
    public async Task CreateTemplate_RotationEntryInsertFails_RollsBackAndDoesNotCommit()
    {
        var user1 = Guid.NewGuid();
        var request = new CreateRecurringTaskRequest("Clean kitchen", "Deep clean", 7, new List<Guid> { user1 });

        _userRepository.GetByIdAsync(user1).Returns(new User { Id = user1 });
        _templateRepository.CreateAsync(Arg.Any<RecurringTaskTemplate>()).Returns(callInfo =>
        {
            var t = callInfo.Arg<RecurringTaskTemplate>();
            t.Id = Guid.NewGuid();
            return t;
        });
        _rotationEntryRepository.CreateAsync(Arg.Any<RotationEntry>())
            .Returns(System.Threading.Tasks.Task.FromException(new InvalidOperationException("insert failed")));

        var act = () => _sut.CreateTemplateAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _unitOfWork.Received(1).RollbackAsync();
        await _unitOfWork.DidNotReceive().CommitAsync();
    }

    [Fact]
    public async Task GenerateNextTask_AssignsToCurrentAndAdvancesIndex()
    {
        var templateId = Guid.NewGuid();
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var user3 = Guid.NewGuid();

        var template = new RecurringTaskTemplate
        {
            Id = templateId,
            Title = "Clean kitchen",
            FrequencyDays = 7,
            CurrentAssigneeIndex = 1
        };
        _templateRepository.GetByIdAsync(templateId).Returns(template);

        var entries = new List<RotationEntry>
        {
            new() { UserId = user1, RotationOrder = 0 },
            new() { UserId = user2, RotationOrder = 1 },
            new() { UserId = user3, RotationOrder = 2 }
        };
        _rotationEntryRepository.GetByTemplateIdAsync(templateId).Returns(entries);

        _taskRepository.CreateAsync(Arg.Any<HouseholdTask>()).Returns(callInfo =>
        {
            var t = callInfo.Arg<HouseholdTask>();
            t.Id = Guid.NewGuid();
            return t;
        });
        _templateRepository.UpdateAsync(Arg.Any<RecurringTaskTemplate>()).Returns(callInfo => callInfo.Arg<RecurringTaskTemplate>());

        var result = await _sut.GenerateNextTaskAsync(templateId);

        result.AssignedToUserId.Should().Be(user2);
        result.TaskType.Should().Be(HouseholdTaskType.Recurring);
        result.TemplateId.Should().Be(templateId);
        await _templateRepository.Received(1).UpdateAsync(Arg.Is<RecurringTaskTemplate>(t => t.CurrentAssigneeIndex == 2));
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task GenerateNextTask_WrapsIndexAtEnd()
    {
        var templateId = Guid.NewGuid();
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        var template = new RecurringTaskTemplate
        {
            Id = templateId,
            Title = "Trash",
            FrequencyDays = 3,
            CurrentAssigneeIndex = 1
        };
        _templateRepository.GetByIdAsync(templateId).Returns(template);

        var entries = new List<RotationEntry>
        {
            new() { UserId = user1, RotationOrder = 0 },
            new() { UserId = user2, RotationOrder = 1 }
        };
        _rotationEntryRepository.GetByTemplateIdAsync(templateId).Returns(entries);

        _taskRepository.CreateAsync(Arg.Any<HouseholdTask>()).Returns(callInfo =>
        {
            var t = callInfo.Arg<HouseholdTask>();
            t.Id = Guid.NewGuid();
            return t;
        });
        _templateRepository.UpdateAsync(Arg.Any<RecurringTaskTemplate>()).Returns(callInfo => callInfo.Arg<RecurringTaskTemplate>());

        var result = await _sut.GenerateNextTaskAsync(templateId);

        result.AssignedToUserId.Should().Be(user2);
        await _templateRepository.Received(1).UpdateAsync(Arg.Is<RecurringTaskTemplate>(t => t.CurrentAssigneeIndex == 0));
    }

    [Fact]
    public async Task GenerateNextTask_TemplateNotFound_ThrowsNotFoundException()
    {
        _templateRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((RecurringTaskTemplate?)null);

        var act = () => _sut.GenerateNextTaskAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
