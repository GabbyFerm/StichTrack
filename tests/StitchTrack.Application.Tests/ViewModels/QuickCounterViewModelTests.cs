using FluentAssertions;
using Moq;
using NUnit.Framework;
using StitchTrack.Application.Interfaces;
using StitchTrack.Application.ViewModels;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;

namespace StitchTrack.Application.Tests.ViewModels;

[TestFixture]
public class QuickCounterViewModelTests
{
    private QuickCounterViewModel _viewModel;
    private Mock<IProjectRepository> _mockProjectRepo;
    private Mock<IDialogService> _mockDialogService;

    [SetUp]
    public void Setup()
    {
        // Create mock repositories and services
        _mockProjectRepo = new Mock<IProjectRepository>();
        _mockDialogService = new Mock<IDialogService>();

        // Create ViewModel with mocked dependencies
        _viewModel = new QuickCounterViewModel(
            _mockProjectRepo.Object,
            _mockDialogService.Object);
    }

    [Test]
    public void Constructor_ShouldInitializeCounterAtZero()
    {
        // Assert
        _viewModel.CurrentCount.Should().Be(0);
    }

    [Test]
    public void Constructor_ShouldInitializeCanSaveToFalse()
    {
        // Assert - Can't save when counter is at 0
        _viewModel.CanSave.Should().BeFalse();
    }

    [Test]
    public void IncrementCommand_ShouldIncreaseCount()
    {
        // Act
        _viewModel.IncrementCommand.Execute(null);

        // Assert
        _viewModel.CurrentCount.Should().Be(1);
    }

    [Test]
    public void IncrementCommand_ShouldEnableSaveButton()
    {
        // Arrange
        _viewModel.CanSave.Should().BeFalse("counter starts at 0");

        // Act
        _viewModel.IncrementCommand.Execute(null);

        // Assert
        _viewModel.CanSave.Should().BeTrue("counter is now > 0");
    }

    [Test]
    public void IncrementCommand_MultipleTimesShouldWork()
    {
        // Act
        _viewModel.IncrementCommand.Execute(null);
        _viewModel.IncrementCommand.Execute(null);
        _viewModel.IncrementCommand.Execute(null);

        // Assert
        _viewModel.CurrentCount.Should().Be(3);
    }

    [Test]
    public void DecrementCommand_ShouldDecreaseCount()
    {
        // Arrange - start at 1
        _viewModel.IncrementCommand.Execute(null);

        // Act
        _viewModel.DecrementCommand.Execute(null);

        // Assert
        _viewModel.CurrentCount.Should().Be(0);
    }

    [Test]
    public void DecrementCommand_WhenAtZero_ShouldStayAtZero()
    {
        // Arrange - counter is already at 0

        // Act
        _viewModel.DecrementCommand.Execute(null);

        // Assert - CRITICAL BUSINESS RULE
        _viewModel.CurrentCount.Should().Be(0);
    }

    [Test]
    public void DecrementCommand_ToZero_ShouldDisableSaveButton()
    {
        // Arrange - increment to 1, then decrement back to 0
        _viewModel.IncrementCommand.Execute(null);
        _viewModel.CanSave.Should().BeTrue("counter is at 1");

        // Act
        _viewModel.DecrementCommand.Execute(null);

        // Assert
        _viewModel.CurrentCount.Should().Be(0);
        _viewModel.CanSave.Should().BeFalse("counter is back at 0");
    }

    [Test]
    public void ResetCommand_ShouldSetCountToZero()
    {
        // Arrange
        _viewModel.IncrementCommand.Execute(null);
        _viewModel.IncrementCommand.Execute(null);
        _viewModel.IncrementCommand.Execute(null); // Now at 3

        // Act
        _viewModel.ResetCommand.Execute(null);

        // Assert
        _viewModel.CurrentCount.Should().Be(0);
    }

    [Test]
    public void ResetCommand_ShouldDisableSaveButton()
    {
        // Arrange - increment to enable save
        _viewModel.IncrementCommand.Execute(null);
        _viewModel.CanSave.Should().BeTrue();

        // Act
        _viewModel.ResetCommand.Execute(null);

        // Assert
        _viewModel.CanSave.Should().BeFalse("counter was reset to 0");
    }

    [Test]
    public void IncrementCommand_ShouldRaisePropertyChanged_ForCurrentCount()
    {
        // Arrange
        var propertyChangedRaised = false;
        _viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(_viewModel.CurrentCount))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        _viewModel.IncrementCommand.Execute(null);

        // Assert - ensures UI updates
        propertyChangedRaised.Should().BeTrue();
    }

    [Test]
    public void IncrementCommand_ShouldRaisePropertyChanged_ForCanSave()
    {
        // Arrange
        var canSaveChangedRaised = false;
        _viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(_viewModel.CanSave))
            {
                canSaveChangedRaised = true;
            }
        };

        // Act
        _viewModel.IncrementCommand.Execute(null);

        // Assert - ensures Save button state updates
        canSaveChangedRaised.Should().BeTrue();
    }

    [Test]
    public async Task SaveToProjectCommand_WithValidName_ShouldSaveProject()
    {
        // Arrange
        _viewModel.IncrementCommand.Execute(null);
        _viewModel.IncrementCommand.Execute(null);
        _viewModel.IncrementCommand.Execute(null); // Counter at 3

        _mockDialogService
            .Setup(x => x.ShowPromptAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()))
            .ReturnsAsync("Test Project");

        // Act
        _viewModel.SaveToProjectCommand.Execute(null);

        // Wait for async operation
        await Task.Delay(200).ConfigureAwait(false);

        // Assert
        _mockProjectRepo.Verify(
            x => x.AddAsync(It.Is<Project>(p => p.Name == "Test Project" && p.CurrentCount == 3)),
            Times.Once);
        _mockProjectRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task SaveToProjectCommand_WithEmptyName_ShouldNotSave()
    {
        // Arrange
        _viewModel.IncrementCommand.Execute(null);

        _mockDialogService
            .Setup(x => x.ShowPromptAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()))
            .ReturnsAsync(string.Empty);

        // Act
        _viewModel.SaveToProjectCommand.Execute(null);

        // Wait for async operation
        await Task.Delay(200).ConfigureAwait(false);

        // Assert
        _mockProjectRepo.Verify(x => x.AddAsync(It.IsAny<Project>()), Times.Never);
    }

    [Test]
    public async Task SaveToProjectCommand_WithTooLongName_ShouldShowError()
    {
        // Arrange
        _viewModel.IncrementCommand.Execute(null);

        var longName = new string('A', 201); // 201 chars (max is 200)
        _mockDialogService
            .Setup(x => x.ShowPromptAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()))
            .ReturnsAsync(longName);

        // Act
        _viewModel.SaveToProjectCommand.Execute(null);

        // Wait for async operation
        await Task.Delay(200).ConfigureAwait(false);

        // Assert
        _mockDialogService.Verify(
            x => x.ShowAlertAsync("Name Too Long", It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
        _mockProjectRepo.Verify(x => x.AddAsync(It.IsAny<Project>()), Times.Never);
    }

    [Test]
    public async Task SaveToProjectCommand_ShouldResetCounterAfterSave()
    {
        // Arrange
        _viewModel.IncrementCommand.Execute(null);
        _viewModel.IncrementCommand.Execute(null); // Counter at 2

        _mockDialogService
            .Setup(x => x.ShowPromptAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()))
            .ReturnsAsync("Saved Project");

        // Act
        _viewModel.SaveToProjectCommand.Execute(null);

        // Wait for async operation
        await Task.Delay(200).ConfigureAwait(false);

        // Assert - counter should be reset to 0
        _viewModel.CurrentCount.Should().Be(0);
        _viewModel.CanSave.Should().BeFalse("counter was reset");
    }

    [Test]
    public async Task SaveToProjectCommand_ShouldShowSuccessToast()
    {
        // Arrange
        _viewModel.IncrementCommand.Execute(null);

        _mockDialogService
            .Setup(x => x.ShowPromptAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()))
            .ReturnsAsync("My Project");

        // Act
        _viewModel.SaveToProjectCommand.Execute(null);

        // Wait for async operation
        await Task.Delay(200).ConfigureAwait(false);

        // Assert
        _mockDialogService.Verify(
            x => x.ShowToastAsync(It.Is<string>(s => s.Contains("My Project"))),
            Times.Once);
    }

    [Test]
    public async Task SaveToProjectCommand_WhenDatabaseFails_ShouldShowError()
    {
        // Arrange
        _viewModel.IncrementCommand.Execute(null);

        _mockDialogService
            .Setup(x => x.ShowPromptAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()))
            .ReturnsAsync("Test Project");

        _mockProjectRepo
            .Setup(x => x.AddAsync(It.IsAny<Project>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        _viewModel.SaveToProjectCommand.Execute(null);

        // Wait for async operation
        await Task.Delay(200).ConfigureAwait(false);

        // Assert
        _mockDialogService.Verify(
            x => x.ShowAlertAsync("Save Failed", It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public void HapticFeedback_ShouldBeTriggeredOnIncrement()
    {
        // Arrange
        var hapticTriggered = false;
        _viewModel.TriggerHapticFeedback = () => hapticTriggered = true;

        // Act
        _viewModel.IncrementCommand.Execute(null);

        // Assert
        hapticTriggered.Should().BeTrue("haptic should trigger on increment");
    }

    [Test]
    public void HapticFeedback_ShouldBeTriggeredOnDecrement()
    {
        // Arrange
        var hapticTriggered = false;
        _viewModel.TriggerHapticFeedback = () => hapticTriggered = true;
        _viewModel.IncrementCommand.Execute(null); // Need to increment first

        // Act
        _viewModel.DecrementCommand.Execute(null);

        // Assert
        hapticTriggered.Should().BeTrue("haptic should trigger on decrement");
    }

    [Test]
    public void HapticFeedback_ShouldBeTriggeredOnReset()
    {
        // Arrange
        var hapticTriggered = false;
        _viewModel.TriggerHapticFeedback = () => hapticTriggered = true;
        _viewModel.IncrementCommand.Execute(null); // Increment first

        // Act
        _viewModel.ResetCommand.Execute(null);

        // Assert
        hapticTriggered.Should().BeTrue("haptic should trigger on reset");
    }
}
