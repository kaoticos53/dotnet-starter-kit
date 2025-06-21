using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Domain.Tests.Common;

namespace FSH.Starter.WebApi.Catalog.Domain.Tests.Entities;

/// <summary>
/// Contiene pruebas unitarias para la entidad Brand.
/// </summary>
/// <summary>
/// Contains unit tests for the Brand entity.
/// </summary>
public sealed class BrandTests : TestBase
{
    private readonly ITestOutputHelper _output;

    public BrandTests(ITestOutputHelper output) : base(output)
    {
        _output = output;
    }

    [Fact]
    public void CreateWithValidParametersShouldCreateBrand()
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
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateWithInvalidNameShouldThrowException(string invalidName)
    {
        // Arrange
        var description = Faker.Lorem.Sentence();

        // Act & Assert
        var action = () => Brand.Create(invalidName, description);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateWithValidParametersShouldUpdateBrand()
    {
        // Arrange
        var brand = Brand.Create("Initial Brand", "Initial Description");
        var newName = "Updated Brand";
        var newDescription = "Updated Description";

        // Act
        var updatedBrand = brand.Update(newName, newDescription);

        // Assert
        updatedBrand.Should().BeSameAs(brand); // Should return the same instance
        brand.Name.Should().Be(newName);
        brand.Description.Should().Be(newDescription);
        // Note: Cannot verify domain events as they are not accessible
    }

    [Fact]
    public void UpdateWithSameValuesShouldNotUpdateBrand()
    {
        // Arrange
        var name = "Test Brand";
        var description = "Test Description";
        var brand = Brand.Create(name, description);
        
        // Act
        var updatedBrand = brand.Update(name, description);

        // Assert
        updatedBrand.Should().BeSameAs(brand);
        brand.Name.Should().Be(name);
        brand.Description.Should().Be(description);
    }

    [Fact]
    public void UpdateWithPartialValuesShouldOnlyUpdateProvidedFields()
    {
        // Arrange
        var originalName = "Original Brand";
        var originalDescription = "Original Description";
        var brand = Brand.Create(originalName, originalDescription);
        
        // Act - Update only description
        var updatedBrand = brand.Update(null, "Updated Description");

        // Assert
        updatedBrand.Should().BeSameAs(brand);
        brand.Name.Should().Be(originalName); // Should remain unchanged
        brand.Description.Should().Be("Updated Description"); // Should be updated
    }

    [Fact]
    public void UpdateWithEmptyNameShouldNotUpdateName()
    {
        // Arrange
        var brand = Brand.Create("Test Brand", "Test Description");
        var originalName = brand.Name;
        var newDescription = "Updated Description";

        // Act & Assert
        var action = () => brand.Update(string.Empty, newDescription);
        action.Should().Throw<ArgumentException>();
        
        // Verify brand was not modified
        brand.Name.Should().Be(originalName);
        brand.Description.Should().NotBe(newDescription);
    }

    [Fact]
    public void UpdateWithWhitespaceNameShouldNotUpdateName()
    {
        // Arrange
        var brand = Brand.Create("Test Brand", "Test Description");
        var originalName = brand.Name;
        var newDescription = "Updated Description";

        // Act & Assert
        var action = () => brand.Update("   ", newDescription);
        action.Should().Throw<ArgumentException>();
        
        // Verify brand was not modified
        brand.Name.Should().Be(originalName);
        brand.Description.Should().NotBe(newDescription);
    }
}
