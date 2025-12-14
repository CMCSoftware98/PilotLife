using PilotLife.Domain.Extensions;

namespace PilotLife.UnitTests.Extensions;

public class GuidExtensionsTests
{
    [Fact]
    public void GetTimestamp_WithVersion7Guid_ReturnsApproximateCreationTime()
    {
        // Arrange
        var beforeCreation = DateTimeOffset.UtcNow;
        var uuid = Guid.CreateVersion7();
        var afterCreation = DateTimeOffset.UtcNow;

        // Act
        var timestamp = uuid.GetTimestamp();

        // Assert
        Assert.InRange(timestamp, beforeCreation.AddMilliseconds(-1), afterCreation.AddMilliseconds(1));
    }

    [Fact]
    public void GetTimestamp_WithSpecificTimestamp_ReturnsCorrectTime()
    {
        // Arrange
        var expectedTime = new DateTimeOffset(2024, 6, 15, 12, 30, 45, TimeSpan.Zero);
        var uuid = Guid.CreateVersion7(expectedTime);

        // Act
        var timestamp = uuid.GetTimestamp();

        // Assert
        // UUID v7 has millisecond precision
        Assert.Equal(expectedTime.ToUnixTimeMilliseconds(), timestamp.ToUnixTimeMilliseconds());
    }

    [Fact]
    public void IsVersion7_WithVersion7Guid_ReturnsTrue()
    {
        // Arrange
        var uuid = Guid.CreateVersion7();

        // Act
        var result = uuid.IsVersion7();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsVersion7_WithVersion4Guid_ReturnsFalse()
    {
        // Arrange
        var uuid = Guid.NewGuid();

        // Act
        var result = uuid.IsVersion7();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsVersion7_WithEmptyGuid_ReturnsFalse()
    {
        // Arrange
        var uuid = Guid.Empty;

        // Act
        var result = uuid.IsVersion7();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetTimestamp_MultipleGuidsCreatedSequentially_AreChronologicallyOrdered()
    {
        // Arrange
        var uuids = Enumerable.Range(0, 10)
            .Select(_ =>
            {
                Thread.Sleep(1); // Ensure different timestamps
                return Guid.CreateVersion7();
            })
            .ToList();

        // Act
        var timestamps = uuids.Select(u => u.GetTimestamp()).ToList();

        // Assert
        for (int i = 1; i < timestamps.Count; i++)
        {
            Assert.True(timestamps[i] >= timestamps[i - 1],
                $"Timestamp at index {i} ({timestamps[i]}) should be >= timestamp at index {i - 1} ({timestamps[i - 1]})");
        }
    }

    [Fact]
    public void GetTimestamp_ReturnsUtcTime()
    {
        // Arrange
        var uuid = Guid.CreateVersion7();

        // Act
        var timestamp = uuid.GetTimestamp();

        // Assert
        Assert.Equal(TimeSpan.Zero, timestamp.Offset);
    }
}
