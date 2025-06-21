using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Domain.Events;
using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Domain.Tests.Domain.Events;

public class BrandEventsTests
{
    [Fact]
    public void BrandCreated_WhenCreated_HasCorrectProperties()
    {
        // Arrange
        var brand = Brand.Create("Test Brand", "Test Description");
        
        // Act
        var @event = new BrandCreated { Brand = brand };
        
        // Assert
        @event.Should().NotBeNull();
        @event.Brand.Should().BeSameAs(brand);
        BrandCreated.EventType.Should().Be(nameof(BrandCreated));
        @event.RaisedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
    
    [Fact]
    public void BrandUpdated_WhenCreated_HasCorrectProperties()
    {
        // Arrange
        var brand = Brand.Create("Test Brand", "Test Description");
        
        // Act
        var @event = new BrandUpdated { Brand = brand };
        
        // Assert
        @event.Should().NotBeNull();
        @event.Brand.Should().BeSameAs(brand);
        BrandUpdated.EventType.Should().Be(nameof(BrandUpdated));
        @event.RaisedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
    
    [Fact]
    public void BrandCreated_WithNullBrand_ThrowsNoException()
    {
        // Act
        var @event = new BrandCreated { Brand = null! };
        
        // Assert
        @event.Brand.Should().BeNull();
    }
    
    [Fact]
    public void BrandUpdated_WithNullBrand_ThrowsNoException()
    {
        // Act
        var @event = new BrandUpdated { Brand = null! };
        
        // Assert
        @event.Brand.Should().BeNull();
    }
}
