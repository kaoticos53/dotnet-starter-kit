using System;
using System.ComponentModel;
using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Products.Create.v1;

public class CreateProductCommandTests
{
    [Fact]
    public void Constructor_WithValidParameters_SetsProperties()
    {
        // Arrange
        var name = "Test Product";
        var description = "Test Description";
        var price = 9.99m;
        var brandId = Guid.NewGuid();

        // Act
        var command = new CreateProductCommand(name, price, description, brandId);

        // Assert
        command.Name.Should().Be(name);
        command.Description.Should().Be(description);
        command.Price.Should().Be(price);
        command.BrandId.Should().Be(brandId);
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsNoException()
    {
        // Act
        var command = new CreateProductCommand(null, 9.99m);

        // Assert
        command.Name.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullDescription_ThrowsNoException()
    {
        // Act
        var command = new CreateProductCommand("Test Product", 9.99m, null);

        // Assert
        command.Description.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullBrandId_ThrowsNoException()
    {
        // Act
        var command = new CreateProductCommand("Test Product", 9.99m, "Description", null);

        // Assert
        command.BrandId.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithDefaultPrice_ThrowsNoException()
    {
        // Act
        var command = new CreateProductCommand("Test Product", 0);

        // Assert
        command.Price.Should().Be(0);
    }

    [Fact]
    public void CreateProductCommand_ImplementsIRequestOfCreateProductResponse()
    {
        // Arrange & Act
        var command = new CreateProductCommand("Test", 9.99m);

        // Assert
        command.Should().BeAssignableTo<MediatR.IRequest<CreateProductResponse>>();
    }

    [Fact]
    public void CreateProductCommand_IsRecord()
    {
        // Arrange & Act
        var command = new CreateProductCommand("Test", 9.99m);

        // Assert
        command.GetType().IsValueType.Should().BeFalse();
        command.GetType().IsClass.Should().BeTrue();
        command.GetType().IsSealed.Should().BeTrue();
    }

    [Fact]
    public void CreateProductCommand_WithSameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command1 = new CreateProductCommand("Test", 9.99m, "Desc", id);
        var command2 = new CreateProductCommand("Test", 9.99m, "Desc", id);

        // Act & Assert
        command1.Should().Be(command2);
        (command1 == command2).Should().BeTrue();
        command1.Equals(command2).Should().BeTrue();
    }
    
    [Fact]
    public void CreateProductCommand_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var command1 = new CreateProductCommand("Test 1", 9.99m, "Desc 1");
        var command2 = new CreateProductCommand("Test 2", 19.99m, "Desc 2");

        // Act & Assert
        command1.Should().NotBe(command2);
        (command1 != command2).Should().BeTrue();
        command1.Equals(command2).Should().BeFalse();
    }
    
    [Fact]
    public void CreateProductCommand_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new CreateProductCommand("Test", 9.99m, "Description", id);

        // Act
        var result = command.ToString();

        // Assert
        result.Should().Be($"CreateProductCommand {{ Name = Test, Price = 9.99, Description = Description, BrandId = {id} }}");
    }
    
    [Fact]
    public void CreateProductCommand_HasDefaultValueAttributes()
    {
        // Arrange
        var nameProperty = typeof(CreateProductCommand).GetProperty(nameof(CreateProductCommand.Name));
        var priceProperty = typeof(CreateProductCommand).GetProperty(nameof(CreateProductCommand.Price));
        var descriptionProperty = typeof(CreateProductCommand).GetProperty(nameof(CreateProductCommand.Description));
        var brandIdProperty = typeof(CreateProductCommand).GetProperty(nameof(CreateProductCommand.BrandId));

        // Assert
        nameProperty.Should().BeDecoratedWith<DefaultValueAttribute>(a => (string?)a.Value == "Sample Product");
        priceProperty.Should().BeDecoratedWith<DefaultValueAttribute>(a => (decimal?)a.Value == 10m);
        descriptionProperty.Should().BeDecoratedWith<DefaultValueAttribute>(a => (string?)a.Value == "Descriptive Description");
        brandIdProperty.Should().BeDecoratedWith<DefaultValueAttribute>(a => a.Value == null);
    }
}
