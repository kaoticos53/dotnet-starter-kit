using System;
using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Domain.Events;
using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Domain.Tests.Domain;

public class BrandTests
{
    [Fact]
    public void Create_WithValidParameters_ReturnsBrand()
    {
        // Arrange
        var name = "Test Brand";
        var description = "Test Description";

        // Act
        var brand = Brand.Create(name, description);

        // Assert
        brand.Should().NotBeNull();
        brand.Id.Should().NotBe(Guid.Empty);
        brand.Name.Should().Be(name);
        brand.Description.Should().Be(description);
        brand.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<BrandCreated>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ThrowsArgumentException(string invalidName)
    {
        // Act & Assert
        var act = () => Brand.Create(invalidName, "Description");
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name cannot be null or whitespace.*")
            .WithParameterName("name");
    }

    [Fact]
    public void Update_WithValidParameters_UpdatesProperties()
    {
        // Arrange
        var brand = Brand.Create("Old Name", "Old Description");
        var newName = "New Name";
        var newDescription = "New Description";
        
        // Clear domain events from creation
        brand.ClearDomainEvents();

        // Act
        var result = brand.Update(newName, newDescription);

        // Assert
        result.Should().BeSameAs(brand);
        brand.Name.Should().Be(newName);
        brand.Description.Should().Be(newDescription);
        brand.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<BrandUpdated>();
    }

    [Fact]
    public void Update_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var brand = Brand.Create("Test", "Description");
        
        // Act & Assert
        var act = () => brand.Update(null, "Description");
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name cannot be empty or whitespace.*")
            .WithParameterName("name");
    }

    [Fact]
    public void Update_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var brand = Brand.Create("Test", "Description");
        
        // Act & Assert
        var act = () => brand.Update("", "Description");
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name cannot be empty or whitespace.*")
            .WithParameterName("name");
    }

    [Fact]
    public void Update_WithSameValues_DoesNotRaiseEvent()
    {
        // Arrange
        var name = "Test Brand";
        var description = "Test Description";
        var brand = Brand.Create(name, description);
        
        // Clear domain events from creation
        brand.ClearDomainEvents();

        // Act
        var result = brand.Update(name, description);

        // Assert
        result.Should().BeSameAs(brand);
        brand.Name.Should().Be(name);
        brand.Description.Should().Be(description);
        brand.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Update_WithNullValues_OnlyUpdatesProvidedValues()
    {
        // Arrange
        var originalName = "Original Name";
        var originalDescription = "Original Description";
        var brand = Brand.Create(originalName, originalDescription);
        var newName = "New Name";
        
        // Clear domain events from creation
        brand.ClearDomainEvents();

        // Act
        var result = brand.Update(newName, null);

        // Assert
        result.Should().BeSameAs(brand);
        brand.Name.Should().Be(newName);
        brand.Description.Should().Be(originalDescription);
        brand.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<BrandUpdated>();
    }
}
