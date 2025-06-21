using System.ComponentModel;
using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Brands.Create.v1;

public class CreateBrandCommandTests
{
    [Fact]
    public void Constructor_WithName_SetsProperties()
    {
        // Arrange
        var name = "Test Brand";
        var description = "Test Description";

        // Act
        var command = new CreateBrandCommand(name, description);

        // Assert
        command.Name.Should().Be(name);
        command.Description.Should().Be(description);
    }

    [Fact]
    public void Constructor_WithNullName_SetsDefaultValue()
    {
        // Act
        var command = new CreateBrandCommand(null);

        // Assert
        command.Name.Should().Be("Sample Brand");
        command.Description.Should().Be("Descriptive Description");
    }

    [Fact]
    public void Constructor_HasDefaultValueAttributes()
    {
        // Arrange & Act
        var nameProperty = typeof(CreateBrandCommand).GetProperty(nameof(CreateBrandCommand.Name));
        var descriptionProperty = typeof(CreateBrandCommand).GetProperty(nameof(CreateBrandCommand.Description));

        // Assert
        nameProperty.Should().NotBeNull();
        nameProperty!.GetCustomAttributes(typeof(DefaultValueAttribute), true)
            .Should().ContainSingle()
            .Which.Should().BeOfType<DefaultValueAttribute>()
            .Which.Value.Should().Be("Sample Brand");

        descriptionProperty.Should().NotBeNull();
        descriptionProperty!.GetCustomAttributes(typeof(DefaultValueAttribute), true)
            .Should().ContainSingle()
            .Which.Should().BeOfType<DefaultValueAttribute>()
            .Which.Value.Should().Be("Descriptive Description");
    }
}
