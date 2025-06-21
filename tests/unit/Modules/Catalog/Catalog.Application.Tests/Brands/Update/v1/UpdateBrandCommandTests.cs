using System;
using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Brands.Update.v1;

public class UpdateBrandCommandTests
{
    [Fact]
    public void Constructor_WithValidParameters_SetsProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Updated Brand";
        var description = "Updated Description";

        // Act
        var command = new UpdateBrandCommand(id, name, description);

        // Assert
        command.Id.Should().Be(id);
        command.Name.Should().Be(name);
        command.Description.Should().Be(description);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsNoException()
    {
        // Act
        var command = new UpdateBrandCommand(Guid.Empty, "Name");

        // Assert
        command.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsNoException()
    {
        // Act
        var command = new UpdateBrandCommand(Guid.NewGuid(), null);

        // Assert
        command.Name.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullDescription_ThrowsNoException()
    {
        // Act
        var command = new UpdateBrandCommand(Guid.NewGuid(), "Name", null);

        // Assert
        command.Description.Should().BeNull();
    }

    [Fact]
    public void UpdateBrandCommand_ImplementsIRequestOfUpdateBrandResponse()
    {
        // Arrange
        var command = new UpdateBrandCommand(Guid.NewGuid(), "Name");

        // Assert
        command.Should().BeAssignableTo<MediatR.IRequest<UpdateBrandResponse>>();
    }
}
