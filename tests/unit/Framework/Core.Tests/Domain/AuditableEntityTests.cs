using System;
using FSH.Framework.Core.Domain;
using FluentAssertions;
using Xunit;

namespace FSH.Framework.Core.Tests.Domain;

public class TestAuditableEntity : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}

public class TestAuditableEntityWithIntId : AuditableEntity<int>
{
    public string Name { get; set; } = string.Empty;
}

public class AuditableEntityTests
{
    [Fact]
    public void Constructor_WhenCalled_InitializesProperties()
    {
        // Arrange & Act
        var entity = new TestAuditableEntity();

        // Assert
        entity.Id.Should().NotBe(Guid.Empty);
        entity.Created.Should().Be(default);
        entity.CreatedBy.Should().Be(Guid.Empty);
        entity.LastModified.Should().Be(default);
        entity.LastModifiedBy.Should().BeNull();
        entity.Deleted.Should().BeNull();
        entity.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void AuditableEntity_WhenCreated_HasNonEmptyGuidId()
    {
        // Arrange & Act
        var entity = new TestAuditableEntity();

        // Assert
        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void AuditableEntityTId_WhenCreated_HasDefaultId()
    {
        // Arrange & Act
        var entity = new TestAuditableEntityWithIntId();

        // Assert
        entity.Id.Should().Be(0);
    }

    
    [Fact]
    public void Properties_WhenSet_CanBeRead()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var userId = Guid.NewGuid();
        var entity = new TestAuditableEntity();

        // Act
        entity.Created = now;
        entity.CreatedBy = userId;
        entity.LastModified = now.AddMinutes(1);
        entity.LastModifiedBy = userId;
        entity.Deleted = now.AddDays(1);
        entity.DeletedBy = userId;

        // Assert
        entity.Created.Should().Be(now);
        entity.CreatedBy.Should().Be(userId);
        entity.LastModified.Should().Be(now.AddMinutes(1));
        entity.LastModifiedBy.Should().Be(userId);
        entity.Deleted.Should().Be(now.AddDays(1));
        entity.DeletedBy.Should().Be(userId);
    }
}
