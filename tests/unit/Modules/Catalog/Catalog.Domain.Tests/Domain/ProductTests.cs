using System;
using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Domain.Events;
using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Domain.Tests.Domain;

public class ProductTests
{
    [Fact]
    public void Create_WithValidParameters_ReturnsProduct()
    {
        // Arrange
        var name = "Test Product";
        var description = "Test Description";
        var price = 99.99m;
        var brandId = Guid.NewGuid();

        // Act
        var product = Product.Create(name, description, price, brandId);

        // Assert
        product.Should().NotBeNull();
        product.Id.Should().NotBe(Guid.Empty);
        product.Name.Should().Be(name);
        product.Description.Should().Be(description);
        product.Price.Should().Be(price);
        product.BrandId.Should().Be(brandId);
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreated>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ThrowsArgumentException(string invalidName)
    {
        // Arrange
        var price = 99.99m;

        // Act & Assert
        var act = () => Product.Create(invalidName, "Description", price, null);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name cannot be null or whitespace.*")
            .WithParameterName("name");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Create_WithInvalidPrice_ThrowsArgumentException(decimal invalidPrice)
    {
        // Arrange & Act
        var act = () => Product.Create("Test Product", "Description", invalidPrice, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Price must be greater than zero.*")
            .WithParameterName("price");
    }

    [Fact]
    public void Update_WithValidParameters_UpdatesProperties()
    {
        // Arrange
        var product = Product.Create("Old Name", "Old Description", 50m, null);
        var newName = "New Name";
        var newDescription = "New Description";
        var newPrice = 100m;
        var newBrandId = Guid.NewGuid();
        
        // Clear domain events from creation
        product.ClearDomainEvents();

        // Act
        var result = product.Update(newName, newDescription, newPrice, newBrandId);

        // Assert
        result.Should().BeSameAs(product);
        product.Name.Should().Be(newName);
        product.Description.Should().Be(newDescription);
        product.Price.Should().Be(newPrice);
        product.BrandId.Should().Be(newBrandId);
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductUpdated>();
    }

    [Fact]
    public void Update_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var product = Product.Create("Test", "Description", 10m, null);
        
        // Act & Assert
        var act = () => product.Update(null, "Description", 10m, null);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name cannot be empty or whitespace.*")
            .WithParameterName("name");
    }

    [Fact]
    public void Update_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var product = Product.Create("Test", "Description", 10m, null);
        
        // Act & Assert
        var act = () => product.Update("", "Description", 10m, null);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name cannot be empty or whitespace.*")
            .WithParameterName("name");
    }

    [Fact]
    public void Update_WithZeroPrice_ThrowsArgumentException()
    {
        // Arrange
        var product = Product.Create("Test", "Description", 10m, null);
        
        // Act & Assert
        var act = () => product.Update("Test", "Description", 0, null);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Price must be greater than zero.*")
            .WithParameterName("price");
    }

    [Fact]
    public void Update_WithNegativePrice_ThrowsArgumentException()
    {
        // Arrange
        var product = Product.Create("Test", "Description", 10m, null);
        
        // Act & Assert
        var act = () => product.Update("Test", "Description", -10m, null);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Price must be greater than zero.*")
            .WithParameterName("price");
    }

    [Fact]
    public void Update_WithNullValues_OnlyUpdatesProvidedValues()
    {
        // Arrange
        var originalName = "Original Name";
        var originalDescription = "Original Description";
        var originalPrice = 50m;
        var originalBrandId = Guid.NewGuid();
        
        var product = Product.Create(originalName, originalDescription, originalPrice, originalBrandId);
        var newName = "New Name";
        
        // Clear domain events from creation
        product.ClearDomainEvents();

        // Act
        var result = product.Update(newName, null, null, null);

        // Assert
        result.Should().BeSameAs(product);
        product.Name.Should().Be(newName);
        product.Description.Should().Be(originalDescription);
        product.Price.Should().Be(originalPrice);
        product.BrandId.Should().Be(originalBrandId);
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductUpdated>();
    }
}
