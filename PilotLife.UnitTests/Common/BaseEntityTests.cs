using PilotLife.Domain.Common;
using PilotLife.Domain.Extensions;

namespace PilotLife.UnitTests.Common;

public class BaseEntityTests
{
    // Test entity that inherits from BaseEntity
    private class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void NewEntity_HasVersion7Id()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        Assert.True(entity.Id.IsVersion7());
    }

    [Fact]
    public void NewEntity_HasCreatedAtSetToCurrentTime()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;

        // Act
        var entity = new TestEntity();
        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.InRange(entity.CreatedAt, before.AddMilliseconds(-1), after.AddMilliseconds(1));
    }

    [Fact]
    public void NewEntity_HasNullModifiedAt()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        Assert.Null(entity.ModifiedAt);
    }

    [Fact]
    public void IdTimestamp_MatchesCreatedAt()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        // Both should be within milliseconds of each other
        var difference = Math.Abs((entity.IdTimestamp - entity.CreatedAt).TotalMilliseconds);
        Assert.True(difference < 10, $"IdTimestamp and CreatedAt differ by {difference}ms");
    }

    [Fact]
    public void MultipleEntities_HaveUniqueIds()
    {
        // Arrange & Act
        var entities = Enumerable.Range(0, 100)
            .Select(_ => new TestEntity())
            .ToList();

        var uniqueIds = entities.Select(e => e.Id).Distinct().Count();

        // Assert
        Assert.Equal(100, uniqueIds);
    }

    [Fact]
    public void EntityIds_AreChronologicallyOrderable()
    {
        // Arrange
        var entities = new List<TestEntity>();
        for (int i = 0; i < 10; i++)
        {
            Thread.Sleep(1);
            entities.Add(new TestEntity());
        }

        // Act
        var sortedByIdString = entities.OrderBy(e => e.Id.ToString()).ToList();

        // Assert - UUID v7 should sort chronologically when compared as strings
        for (int i = 0; i < entities.Count; i++)
        {
            Assert.Equal(entities[i].Id, sortedByIdString[i].Id);
        }
    }
}
