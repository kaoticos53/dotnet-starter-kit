using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Domain.Events;
using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Domain.Tests.Domain.Events;

public class ProductEventsTests
{
    [Fact]
    public void ProductCreated_WhenCreated_HasCorrectProperties()
    {
        // Arrange
        var product = Product.Create("Test Product", "Test Description", 99.99m, null);
        
        // Act
        var @event = new ProductCreated { Product = product };
        
        // Assert
        @event.Should().NotBeNull();
        @event.Product.Should().BeSameAs(product);
        ProductCreated.EventType.Should().Be(nameof(ProductCreated));
        @event.RaisedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
    
    [Fact]
    public void ProductUpdated_WhenCreated_HasCorrectProperties()
    {
        // Arrange
        var product = Product.Create("Test Product", "Test Description", 99.99m, null);
        
        // Act
        var @event = new ProductUpdated { Product = product };
        
        // Assert
        @event.Should().NotBeNull();
        @event.Product.Should().BeSameAs(product);
        ProductUpdated.EventType.Should().Be(nameof(ProductUpdated));
        @event.RaisedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
    
    [Fact]
    public void ProductCreated_WithNullProduct_ThrowsNoException()
    {
        // Act
        var @event = new ProductCreated { Product = null };
        
        // Assert
        @event.Product.Should().BeNull();
    }
    
    [Fact]
    public void ProductUpdated_WithNullProduct_ThrowsNoException()
    {
        // Act
        var @event = new ProductUpdated { Product = null };
        
        // Assert
        @event.Product.Should().BeNull();
    }
}
