using FluentAssertions;
using NUnit.Framework;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Domain.Tests.Entities;

[TestFixture]
internal class UserTests
{
    [Test]
    public void CreateUser_WithValidDetails_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var email = " test@example.com ";
        var passwordHash = "dummyhash";
        var displayName = " John Doe ";

        var user = User.CreateUser(email, passwordHash, displayName);

        // Assert
        user.Id.Should().NotBe(Guid.Empty);
        user.Email.Should().Be("TEST@EXAMPLE.COM"); // Verifies trimming and ToUpperInvariant
        user.PasswordHash.Should().Be(passwordHash);
        user.DisplayName.Should().Be("John Doe"); // Verifies trimming
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void CreateUser_WithInvalidEmail_ShouldThrowException(string? invalidEmail)
    {
        Action act = () => User.CreateUser(invalidEmail!, "dummyhash");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*email*");
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void CreateUser_WithInvalidPasswordHash_ShouldThrowException(string? invalidHash)
    {
        // Arrange & Act
        Action act = () => User.CreateUser("test@example.com", invalidHash!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*password hash*");
    }

    [Test]
    public void UpdateDisplayName_ShouldUpdateAndTrim()
    {
        // Arrange
        var user = User.CreateUser("test@example.com", "hash", "Old Name");

        // Act
        user.UpdateDisplayName(" New Name ");

        // Assert
        user.DisplayName.Should().Be("New Name");
    }

    [Test]
    public void UpdateDisplayName_WithNull_ShouldSetToNull()
    {
        // Arrange
        var user = User.CreateUser("test@example.com", "hash", "Old Name");

        // Act
        user.UpdateDisplayName(null);

        // Assert
        user.DisplayName.Should().BeNull();
    }
}
