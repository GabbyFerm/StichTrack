using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StitchTrack.Infrastructure.Data;
using StitchTrack.Infrastructure.Repositories;

namespace StitchTrack.IntegrationTests.Repositories;

[TestFixture]
public class AppSettingsRepositoryTests
{
    private AppDbContext _context;
    private AppSettingsRepository _repository;

    [SetUp]
    public void Setup()
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new AppSettingsRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task GetAppSettingsAsync_WhenNoSettingsExist_ShouldCreateDefault()
    {
        // Act
        var settings = await _repository.GetAppSettingsAsync().ConfigureAwait(false);
        await _repository.SaveAppSettingsAsync(settings).ConfigureAwait(false);

        // Assert
        settings.Should().NotBeNull();
        settings.Id.Should().Be(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        settings.IsFirstRun.Should().BeTrue();
        settings.Theme.Should().Be("Auto");
        settings.HapticFeedbackEnabled.Should().BeTrue();
        settings.SyncEnabled.Should().BeFalse();
        settings.ProjectCreationCount.Should().Be(0);
    }

    [Test]
    public async Task GetAppSettingsAsync_WhenCalledMultipleTimes_ShouldReturnSameInstance()
    {
        // Act
        var settings1 = await _repository.GetAppSettingsAsync().ConfigureAwait(false);
        await _repository.SaveAppSettingsAsync(settings1).ConfigureAwait(false);
        var settings2 = await _repository.GetAppSettingsAsync().ConfigureAwait(false);
        await _repository.SaveAppSettingsAsync(settings2).ConfigureAwait(false);

        // Assert - EF Core change tracking returns same instance
        settings1.Id.Should().Be(settings2.Id);
        settings1.Should().BeSameAs(settings2); // ✅ Same instance due to EF Core tracking
    }

    [Test]
    public async Task SaveAppSettingsAsync_ShouldPersistChanges()
    {
        // Arrange
        var settings = await _repository.GetAppSettingsAsync().ConfigureAwait(false);
        await _repository.SaveAppSettingsAsync(settings).ConfigureAwait(false);
        settings.CompleteFirstRun();
        settings.UpdateTheme("Dark");

        // Act
        await _repository.SaveAppSettingsAsync(settings).ConfigureAwait(false);

        // Assert - detach and reload from database to verify persistence
        _context.Entry(settings).State = EntityState.Detached;
        var reloadedSettings = await _repository.GetAppSettingsAsync().ConfigureAwait(false);

        reloadedSettings.IsFirstRun.Should().BeFalse();
        reloadedSettings.FirstRunCompletedAt.Should().NotBeNull();
        reloadedSettings.Theme.Should().Be("Dark");
    }

    [Test]
    public async Task SaveAppSettingsAsync_WhenSyncEnabled_ShouldPersist()
    {
        // Arrange
        var settings = await _repository.GetAppSettingsAsync().ConfigureAwait(false);
        settings.EnableSync("iCloud");

        // Act
        await _repository.SaveAppSettingsAsync(settings).ConfigureAwait(false);

        // Assert - detach and reload
        _context.Entry(settings).State = EntityState.Detached;
        var reloadedSettings = await _repository.GetAppSettingsAsync().ConfigureAwait(false);

        reloadedSettings.SyncEnabled.Should().BeTrue();
        reloadedSettings.SyncProvider.Should().Be("iCloud");
    }

    [Test]
    public async Task SaveAppSettingsAsync_WhenSyncDisabled_ShouldPersist()
    {
        // Arrange
        var settings = await _repository.GetAppSettingsAsync().ConfigureAwait(false);
        settings.EnableSync("GoogleDrive");
        await _repository.SaveAppSettingsAsync(settings).ConfigureAwait(false);

        // Detach to force reload
        _context.Entry(settings).State = EntityState.Detached;

        // Act
        var reloadedSettings = await _repository.GetAppSettingsAsync().ConfigureAwait(false);
        reloadedSettings.DisableSync();
        await _repository.SaveAppSettingsAsync(reloadedSettings).ConfigureAwait(false);

        // Assert - detach and reload again
        _context.Entry(reloadedSettings).State = EntityState.Detached;
        var finalSettings = await _repository.GetAppSettingsAsync().ConfigureAwait(false);

        finalSettings.SyncEnabled.Should().BeFalse();
        finalSettings.SyncProvider.Should().BeNull();
    }

    [Test]
    public async Task SaveAppSettingsAsync_WhenProjectCountIncremented_ShouldPersist()
    {
        // Arrange
        var settings = await _repository.GetAppSettingsAsync().ConfigureAwait(false);
        settings.IncrementProjectCreationCount();
        settings.IncrementProjectCreationCount();
        settings.IncrementProjectCreationCount();

        // Act
        await _repository.SaveAppSettingsAsync(settings).ConfigureAwait(false);

        // Assert - detach and reload
        _context.Entry(settings).State = EntityState.Detached;
        var reloadedSettings = await _repository.GetAppSettingsAsync().ConfigureAwait(false);

        reloadedSettings.ProjectCreationCount.Should().Be(3);
    }

    [Test]
    public void SaveAppSettingsAsync_WhenSettingsIsNull_ShouldThrowException()
    {
        // Act
        Func<Task> act = async () => await _repository.SaveAppSettingsAsync(null!).ConfigureAwait(false);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task GetAppSettingsAsync_WhenSettingsExistInDatabase_ShouldReturnExisting()
    {
        // Arrange - create settings first
        var firstSettings = await _repository.GetAppSettingsAsync().ConfigureAwait(false);
        firstSettings.CompleteFirstRun();
        firstSettings.IncrementProjectCreationCount();
        await _repository.SaveAppSettingsAsync(firstSettings).ConfigureAwait(false);

        // Detach to simulate fresh load
        _context.Entry(firstSettings).State = EntityState.Detached;

        // Act - get settings again (should find existing, not create new)
        var secondSettings = await _repository.GetAppSettingsAsync().ConfigureAwait(false);

        // Assert
        secondSettings.IsFirstRun.Should().BeFalse();
        secondSettings.ProjectCreationCount.Should().Be(1);
        secondSettings.Id.Should().Be(Guid.Parse("00000000-0000-0000-0000-000000000001"));
    }
}
