using System;
using System.Collections.Generic;
using System.Linq;
using FSH.Framework.Core.Domain;
using FSH.Framework.Core.Domain.Events;
using FluentAssertions;
using Xunit;

namespace FSH.Framework.Core.Tests.Domain;

public record TestDomainEvent : DomainEvent
{
    public string TestProperty { get; set; } = string.Empty;
    
    public TestDomainEvent()
    {
        RaisedOn = DateTime.UtcNow;
    }
}

public class TestEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}

public class BaseEntityTests
{
    [Fact]
    public void Constructor_WhenCalled_InitializesDomainEvents()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.DomainEvents.Should().NotBeNull();
        entity.DomainEvents.Should().BeEmpty();
    }

    
    [Fact]
    public void QueueDomainEvent_WithNewEvent_AddsEventToCollection()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent { TestProperty = "Test" };

        // Act
        entity.QueueDomainEvent(domainEvent);

        // Assert
        entity.DomainEvents.Should().ContainSingle();
        entity.DomainEvents.First().Should().Be(domainEvent);
    }

    
    [Fact]
    public void QueueDomainEvent_WithSameEvent_DoesNotAddDuplicate()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent { TestProperty = "Test" };

        // Act
        entity.QueueDomainEvent(domainEvent);
        entity.QueueDomainEvent(domainEvent);

        // Assert
        entity.DomainEvents.Should().ContainSingle();
    }
    
    [Fact]
    public void BaseEntity_WhenCreated_HasNonEmptyGuidId()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().NotBe(Guid.Empty);
    }
    
    [Fact]
    public void BaseEntityTId_WhenCreated_HasDefaultId()
    {
        // Arrange & Act
        var entity = new TestEntityWithIntId();

        // Assert
        entity.Id.Should().Be(0);
    }
}

public class TestEntityWithIntId : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
}
