using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using FSH.Starter.WebApi.Catalog.Domain;
using Catalog.Domain.Tests.Common;

namespace Catalog.Domain.Tests.Entities;

/// <summary>
/// Contiene pruebas unitarias para la entidad Product.
/// </summary>
/// <summary>
/// Contains unit tests for the Product entity.
/// </summary>
public sealed class ProductTests : TestBase
{
    private readonly ITestOutputHelper _output;

    public ProductTests(ITestOutputHelper output) : base(output)
    {
        _output = output;
    }

    [Fact]
    public void CreateWithValidParametersShouldCreateProduct()
    {
        // Arrange
        var name = Faker.Commerce.ProductName();
        var description = Faker.Lorem.Sentence();
        var price = Faker.Random.Decimal(1, 1000);
        var brandId = Guid.NewGuid();

        // Act
        var product = Product.Create(name, description, price, brandId);

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().Be(name);
        product.Description.Should().Be(description);
        product.Price.Should().Be(price);
        product.BrandId.Should().Be(brandId);

    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void CreateWithInvalidPriceShouldThrowException(decimal invalidPrice)
    {
        // Arrange
        var name = Faker.Commerce.ProductName();
        var description = Faker.Lorem.Sentence();
        var brandId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Product.Create(name, description, invalidPrice, brandId));
        exception.ParamName.Should().Be("price");
    }

    [Fact]
    public void UpdateWithValidParametersShouldUpdateProduct()
    {
        // Arrange
        var product = Product.Create("Old Product", "Old Description", 100, Guid.NewGuid());
        var newName = Faker.Commerce.ProductName();
        var newDescription = Faker.Lorem.Sentence();
        var newPrice = Faker.Random.Decimal(1, 1000);
        var newBrandId = Guid.NewGuid();
        
        // Act
        var updatedProduct = product.Update(newName, newDescription, newPrice, newBrandId);

        // Assert
        updatedProduct.Should().BeSameAs(product); // Should return the same instance
        product.Name.Should().Be(newName);
        product.Description.Should().Be(newDescription);
        product.Price.Should().Be(newPrice);
        product.BrandId.Should().Be(newBrandId);
        // Note: Cannot verify domain events as they are not accessible
    }

    [Fact]
    public void UpdateWithPartialValuesShouldOnlyUpdateProvidedFields()
    {
        // Arrange
        var originalName = "Original Product";
        var originalDescription = "Original Description";
        var originalPrice = 100m;
        var originalBrandId = Guid.NewGuid();
        
        var product = Product.Create(originalName, originalDescription, originalPrice, originalBrandId);
        
        // Act - Update only price
        var updatedProduct = product.Update(null, null, 200m, null);

        // Assert
        updatedProduct.Should().BeSameAs(product);
        product.Name.Should().Be(originalName); // Should remain unchanged
        product.Description.Should().Be(originalDescription); // Should remain unchanged
        product.Price.Should().Be(200m); // Should be updated
        product.BrandId.Should().Be(originalBrandId); // Should remain unchanged
    }

    [Fact]
    public void UpdateWithSameValuesShouldNotUpdateProduct()
    {
        // Arrange
        var name = "Test Product";
        var description = "Test Description";
        var price = 100m;
        var brandId = Guid.NewGuid();
        
        var product = Product.Create(name, description, price, brandId);
        
        // Act - Update with same values
        var updatedProduct = product.Update(name, description, price, brandId);

        // Assert
        updatedProduct.Should().BeSameAs(product);
        product.Name.Should().Be(name);
        product.Description.Should().Be(description);
        product.Price.Should().Be(price);
        product.BrandId.Should().Be(brandId);
    }

    [Fact]
    public void UpdateWithInvalidPriceShouldThrowException()
    {
        // Arrange
        var product = Product.Create("Test Product", "Test Description", 100, Guid.NewGuid());
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => product.Update(null, null, 0, null));
        Assert.Throws<ArgumentException>(() => product.Update(null, null, -1, null));
        Assert.Throws<ArgumentException>(() => product.Update(null, null, -100.50m, null));
    }

    [Fact]
    public void UpdateWithInvalidNameShouldThrowException()
    {
        // Arrange
        var product = Product.Create("Test Product", "Test Description", 100, Guid.NewGuid());
        
        // Act & Assert - null name should not throw, empty/whitespace should throw ArgumentException
        product.Invoking(p => p.Update(null, "Updated Description", 200, null)).Should().NotThrow();
        product.Invoking(p => p.Update("", "Updated Description", 200, null)).Should().Throw<ArgumentException>();
        product.Invoking(p => p.Update("   ", "Updated Description", 200, null)).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateWithVeryLargePriceShouldUpdateSuccessfully()
    {
        // Arrange
        var brandId = Guid.NewGuid();
        var product = Product.Create(
            Faker.Commerce.ProductName(), 
            Faker.Lorem.Sentence(), 
            100, 
            brandId);
            
        var newPrice = 1_000_000m;

        // Act
        var updatedProduct = product.Update(
            product.Name, 
            product.Description, 
            newPrice, 
            product.BrandId);

        // Assert
        updatedProduct.Price.Should().Be(newPrice);
    }
}
