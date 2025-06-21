using System;
using FluentAssertions;
using Xunit;
using FSH.Framework.Core.Domain;
using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Domain.Events;
using Catalog.Domain.UnitTests.Tests.Common;

namespace Catalog.Domain.UnitTests.Tests.Entities;

/// <summary>  
/// Contiene pruebas unitarias para la entidad Product.  
/// </summary>  
public class ProductTests : BaseTest
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateProduct()
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
        product.DomainEvents.Should().ContainSingle(e => e is ProductCreated);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidName_ShouldCreateProductWithNullName(string invalidName)
    {
        // Arrange  
        var validDescription = Faker.Lorem.Sentence();
        var validPrice = Faker.Random.Decimal(1, 1000);
        var brandId = Guid.NewGuid();

        // Act  
        var product = Product.Create(invalidName, validDescription, validPrice, brandId);

        // Assert  
        product.Should().NotBeNull();
        product.Name.Should().Be(invalidName); // Accepts null/empty/whitespace as-is
        product.Description.Should().Be(validDescription);
        product.Price.Should().Be(validPrice);
        product.BrandId.Should().Be(brandId);
        product.DomainEvents.Should().ContainSingle(e => e is ProductCreated);
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateProduct()
    {
        // Arrange  
        var product = Product.Create(
            Faker.Commerce.ProductName(),
            Faker.Lorem.Sentence(),
            Faker.Random.Decimal(1, 1000),
            Guid.NewGuid());

        var newName = Faker.Commerce.ProductName();
        var newDescription = Faker.Lorem.Sentence();
        var newPrice = Faker.Random.Decimal(1, 1000);
        var newBrandId = Guid.NewGuid();

        // Clear the initial creation event  
        product.DomainEvents.Clear();

        // Act  
        var updatedProduct = product.Update(newName, newDescription, newPrice, newBrandId);

        // Assert  
        updatedProduct.Name.Should().Be(newName);
        updatedProduct.Description.Should().Be(newDescription);
        updatedProduct.Price.Should().Be(newPrice);
        updatedProduct.BrandId.Should().Be(newBrandId);
        updatedProduct.DomainEvents.Should().ContainSingle(e => e is ProductUpdated);
    }

    // Fix for CS1929: Replace 'ClearDomainEvents()' with the correct method to clear domain events for the Product entity.  
    // Assuming the Product entity has a method or extension to clear domain events, we need to use it correctly.  

    [Fact]
    public void Update_WithNoChanges_ShouldNotRaiseEvent()
    {
        // Arrange  
        var initialName = Faker.Commerce.ProductName();
        var initialDescription = Faker.Lorem.Sentence();
        var initialPrice = Faker.Random.Decimal(1, 1000);
        var initialBrandId = Guid.NewGuid();

        var product = Product.Create(initialName, initialDescription, initialPrice, initialBrandId);

        // Clear the creation event
        product.DomainEvents.Clear();


        // Act  
        var updatedProduct = product.Update(initialName, initialDescription, initialPrice, initialBrandId);

        // Assert  
        updatedProduct.DomainEvents.Should().BeEmpty();
        updatedProduct.Name.Should().Be(initialName);
        updatedProduct.Description.Should().Be(initialDescription);
        updatedProduct.Price.Should().Be(initialPrice);
        updatedProduct.BrandId.Should().Be(initialBrandId);
    }

    [Fact]
    public void Update_WithNullValues_ShouldSetPropertiesToNullExceptoName()
    {
        // Arrange  
        var originalName = Faker.Commerce.ProductName();
        var originalDescription = Faker.Lorem.Sentence();
        var originalPrice = Faker.Random.Decimal(1, 1000);
        var originalBrandId = Guid.NewGuid();
        
        var product = Product.Create(
            originalName,
            originalDescription,
            originalPrice,
            originalBrandId);

        // Clear the creation event  
        product.DomainEvents.Clear();


        // Act  
        var updatedProduct = product.Update(null, null, null, null);


        // Assert  
        updatedProduct.Name.Should().Be(originalName); // Name is set to origialName
        updatedProduct.Description.Should().BeNull(); // Description is set to null
        updatedProduct.Price.Should().Be(originalPrice); // Price remains unchanged
        updatedProduct.BrandId.Should().Be(originalBrandId); // BrandId remains unchanged
        updatedProduct.DomainEvents.Should().ContainSingle(e => e is ProductUpdated);
    }
}
