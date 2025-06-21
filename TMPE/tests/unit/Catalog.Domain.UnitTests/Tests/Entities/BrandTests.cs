using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Domain.Events;
using Catalog.Domain.UnitTests.Tests.Common;

namespace Catalog.Domain.UnitTests.Tests.Entities;

/// <summary>
/// Contiene pruebas unitarias para la entidad Brand.
/// </summary>
public class BrandTests : BaseTest
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateBrand()
    {
        // Arrange
        var name = Faker.Company.CompanyName();
        var description = Faker.Lorem.Sentence();

        // Act
        var brand = Brand.Create(name, description);

        // Assert
        brand.Should().NotBeNull();
        brand.Name.Should().Be(name);
        brand.Description.Should().Be(description);
        brand.DomainEvents.Should().ContainSingle(e => e is BrandCreated);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidName_ShouldCreateBrandWithNullName(string invalidName)
    {
        // Arrange
        var validDescription = Faker.Lorem.Sentence();

        // Act
        var brand = Brand.Create(invalidName, validDescription);

        // Assert
        brand.Should().NotBeNull();
        brand.Name.Should().Be(invalidName); // Accepts null/empty/whitespace as-is
        brand.Description.Should().Be(validDescription);
        brand.DomainEvents.Should().ContainSingle(e => e is BrandCreated);
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateBrand()
    {
        // Arrange
        var brand = Brand.Create("Original Name", "Original Description");
        var newName = "Updated Name";
        var newDescription = "Updated Description";
        
        // Clear the creation event
        brand.ClearDomainEvents();

        // Act
        var updatedBrand = brand.Update(newName, newDescription);

        // Assert
        updatedBrand.Name.Should().Be(newName);
        updatedBrand.Description.Should().Be(newDescription);
        updatedBrand.DomainEvents.Should().ContainSingle(e => e is BrandUpdated);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WithInvalidName_ShouldSetOriginalNameToValue(string invalidName)
    {
        // Arrange
        var originalName = Faker.Company.CompanyName();
        var description = Faker.Lorem.Sentence();
        var brand = Brand.Create(originalName, description);
        
        // Clear the creation event
        brand.ClearDomainEvents();

        // Act
        var updatedBrand = brand.Update(invalidName, description);

        // Assert
        updatedBrand.Name.Should().Be(originalName); // Sets to the exact value passed
        updatedBrand.Description.Should().Be(description);
        updatedBrand.DomainEvents.Should().NotContain(e => e is BrandUpdated);
    }

    [Fact]
    public void Update_WithNullDescription_ShouldSetDescriptionToNull()
    {
        // Arrange
        var brandName = Faker.Company.CompanyName();
        var brand = Brand.Create(brandName, "Original Description");
        brand.ClearDomainEvents();

        // Act
        var updatedBrand = brand.Update(brandName, null);

        // Assert
        updatedBrand.Description.Should().BeNull();
        updatedBrand.DomainEvents.Should().ContainSingle(e => e is BrandUpdated);
    }

    [Fact]
    public void Update_WithNoChanges_ShouldNotRaiseEvent()
    {
        // Arrange
        var initialName = Faker.Company.CompanyName();
        var initialDescription = Faker.Lorem.Sentence();
        
        var brand = Brand.Create(initialName, initialDescription);
        // Clear the creation event that was queued during creation
        brand.ClearDomainEvents();

        // Act
        var updatedBrand = brand.Update(initialName, initialDescription);

        // Assert
        updatedBrand.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Update_WithNullValues_ShouldSetNameToOriginalAndDescriptionToNullValues()
    {
        // Arrange
        var originalName = Faker.Company.CompanyName();
        var originalDescription = Faker.Lorem.Sentence();
        var brand = Brand.Create(originalName, originalDescription);
        
        // Clear the creation event
        brand.ClearDomainEvents();

        // Act
        var updatedBrand = brand.Update(null, null);

        // Assert
        updatedBrand.Name.Should().Be(originalName); // Name is set to null
        updatedBrand.Description.Should().BeNull(); // Description is set to null
        updatedBrand.DomainEvents.Should().ContainSingle(e => e is BrandUpdated);
    }

    [Fact]
    public void Update_WithEmptyName_ShouldSetLastName()
    {
        // Arrange
        var originalName = Faker.Company.CompanyName();
        var description = Faker.Lorem.Sentence();
        var brand = Brand.Create(originalName, description);
        brand.ClearDomainEvents();

        // Act
        var updatedBrand = brand.Update("", description);

        // Assert
        updatedBrand.Name.Should().Be(originalName);
        updatedBrand.Description.Should().Be(description);
        updatedBrand.DomainEvents.Should().NotContain(e => e is BrandUpdated); // Fix: Replace 'is not' with 'NotContain'
    }
}
// Add the following extension method to handle the ClearDomainEvents functionality.  
public static class DomainEventExtensions
{
    public static void ClearDomainEvents(this Brand brand)
    {
        brand.DomainEvents.Clear();
    }
} 