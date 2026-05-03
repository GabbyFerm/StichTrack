using FluentAssertions;
using NUnit.Framework;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Domain.Tests.Entities;

[TestFixture]
internal sealed class AppSettingsTests
{
    [Test]
    public void CreateDefault_ShouldHaveCorrectInitialState()
    {
        // Act
        var settings = AppSettings.CreateDefault();

        // Assert
        settings.IsFirstRun.Should().BeTrue();
        settings.Theme.Should().Be("Auto");
        settings.HapticFeedbackEnabled.Should().BeTrue();
        settings.ProjectCreationCount.Should().Be(0);
        settings.FirstRunCompletedAt.Should().BeNull();
    }

    [Test]
    public void CompleteFirstRun_ShouldUpdateFlags()
    {
        // Arrange
        var settings = AppSettings.CreateDefault();

        // Act
        settings.CompleteFirstRun();

        // Assert
        settings.IsFirstRun.Should().BeFalse();
        settings.FirstRunCompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void ResetFirstRun_ShouldRestoreInitialState()
    {
        // Arrange
        var settings = AppSettings.CreateDefault();
        settings.CompleteFirstRun();

        // Act
        settings.ResetFirstRun();

        // Assert
        settings.IsFirstRun.Should().BeTrue();
        settings.FirstRunCompletedAt.Should().BeNull();
    }

    [Test]
    public void EnableSync_WithValidProvider_ShouldUpdateSettings()
    {
        // Arrange
        var settings = AppSettings.CreateDefault();

        // Act
        settings.EnableSync("iCloud");

        // Assert
        settings.SyncEnabled.Should().BeTrue();
        settings.SyncProvider.Should().Be("iCloud");
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void EnableSync_WithInvalidProvider_ShouldThrowException(string? provider)
    {
        // Arrange
        var settings = AppSettings.CreateDefault();

        // Act
        Action act = () => settings.EnableSync(provider!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Provider*");
    }

    [Test]
    public void DisableSync_ShouldClearSyncSettings()
    {
        // Arrange
        var settings = AppSettings.CreateDefault();
        settings.EnableSync("iCloud");

        // Act
        settings.DisableSync();

        // Assert
        settings.SyncEnabled.Should().BeFalse();
        settings.SyncProvider.Should().BeNull();
    }

    [TestCase("Light")]
    [TestCase("Dark")]
    [TestCase("Auto")]
    public void UpdateTheme_WithValidTheme_ShouldUpdate(string theme)
    {
        // Arrange
        var settings = AppSettings.CreateDefault();

        // Act
        settings.UpdateTheme(theme);

        // Assert
        settings.Theme.Should().Be(theme);
    }

    [Test]
    public void UpdateTheme_WithInvalidTheme_ShouldThrowException()
    {
        // Arrange
        var settings = AppSettings.CreateDefault();

        // Act
        Action act = () => settings.UpdateTheme("Neon");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*theme*");
    }

    [Test]
    public void ToggleHapticFeedback_ShouldFlipState()
    {
        // Arrange
        var settings = AppSettings.CreateDefault();
        settings.HapticFeedbackEnabled.Should().BeTrue();

        // Act & Assert
        settings.ToggleHapticFeedback().Should().BeFalse();
        settings.HapticFeedbackEnabled.Should().BeFalse();

        settings.ToggleHapticFeedback().Should().BeTrue();
        settings.HapticFeedbackEnabled.Should().BeTrue();
    }
}
