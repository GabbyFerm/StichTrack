using FluentAssertions;
using NUnit.Framework;
using StitchTrack.Application.ViewModels;

namespace StitchTrack.Application.Tests.ViewModels;

[TestFixture]
public class GuestCounterViewModelTests
{
    private GuestCounterViewModel _viewModel;

    [SetUp]
    public void Setup()
    {
        // Create fresh ViewModel before each test
        _viewModel = new GuestCounterViewModel();
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
}
