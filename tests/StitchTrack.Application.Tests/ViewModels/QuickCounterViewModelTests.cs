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
    private Mock<IAppSettingsRepository> _mockSettingsRepo;
    private Mock<IProjectRepository> _mockProjectRepo;
    private Mock<IDialogService> _mockDialogService;

    [SetUp]
    public void Setup()
    {
        // Create mock repositories and services
        _mockSettingsRepo = new Mock<IAppSettingsRepository>();
        _mockProjectRepo = new Mock<IProjectRepository>();
        _mockDialogService = new Mock<IDialogService>();

        // Setup default behavior: return default settings
        var defaultSettings = AppSettings.CreateDefault();
        _mockSettingsRepo
            .Setup(x => x.GetAppSettingsAsync())
            .ReturnsAsync(defaultSettings);

        // Create ViewModel with mocked dependencies
        _viewModel = new QuickCounterViewModel(
            _mockSettingsRepo.Object,
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
    public void IncrementCommand_ShouldIncreaseCount()
    {
        // Act
        _viewModel.IncrementCommand.Execute(null);

        // Assert
        _viewModel.CurrentCount.Should().Be(1);
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
    public void IncrementCommand_ShouldRaisePropertyChanged()
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
    public async Task ShowOnboarding_WhenFirstRun_ShouldBeTrue()
    {
        // Arrange
        var settings = AppSettings.CreateDefault();
        var mockSettings = new Mock<IAppSettingsRepository>();
        var mockProject = new Mock<IProjectRepository>();
        var mockDialog = new Mock<IDialogService>();
        mockSettings
            .Setup(x => x.GetAppSettingsAsync())
            .ReturnsAsync(settings);

        // Act
        var viewModel = new QuickCounterViewModel(mockSettings.Object, mockProject.Object, mockDialog.Object);

        // Wait a bit for async initialization
        await Task.Delay(100).ConfigureAwait(false);

        // Assert
        viewModel.ShowOnboarding.Should().BeTrue();
    }

    [Test]
    public async Task ShowOnboarding_WhenNotFirstRun_ShouldBeFalse()
    {
        // Arrange
        var settings = AppSettings.CreateDefault();
        settings.CompleteFirstRun();

        var mockSettings = new Mock<IAppSettingsRepository>();
        var mockProject = new Mock<IProjectRepository>();
        var mockDialog = new Mock<IDialogService>();
        mockSettings
            .Setup(x => x.GetAppSettingsAsync())
            .ReturnsAsync(settings);

        // Act
        var viewModel = new QuickCounterViewModel(mockSettings.Object, mockProject.Object, mockDialog.Object);

        // Wait for async initialization
        await Task.Delay(100).ConfigureAwait(false);

        // Assert
        viewModel.ShowOnboarding.Should().BeFalse();
    }

    [Test]
    public async Task GetStartedCommand_ShouldHideOnboarding()
    {
        // Arrange
        var settings = AppSettings.CreateDefault();
        var mockSettings = new Mock<IAppSettingsRepository>();
        var mockProject = new Mock<IProjectRepository>();
        var mockDialog = new Mock<IDialogService>();
        mockSettings
            .Setup(x => x.GetAppSettingsAsync())
            .ReturnsAsync(settings);

        var viewModel = new QuickCounterViewModel(mockSettings.Object, mockProject.Object, mockDialog.Object);
        await Task.Delay(100).ConfigureAwait(false); // Wait for initialization

        // Act
        viewModel.GetStartedCommand.Execute(null);
        await Task.Delay(100).ConfigureAwait(false); // Wait for async command

        // Assert
        viewModel.ShowOnboarding.Should().BeFalse();
        mockSettings.Verify(x => x.SaveAppSettingsAsync(It.IsAny<AppSettings>()), Times.Once);
    }
}
