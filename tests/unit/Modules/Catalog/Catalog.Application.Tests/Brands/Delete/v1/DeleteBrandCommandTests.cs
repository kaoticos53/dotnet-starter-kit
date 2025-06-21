using System;
using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Brands.Delete.v1;

public class DeleteBrandCommandTests
{
    [Fact]
    public void Constructor_WithValidId_SetsIdProperty()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = new DeleteBrandCommand(id);

        // Assert
        command.Id.Should().Be(id);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsNoException()
    {
        // Act
        var command = new DeleteBrandCommand(Guid.Empty);

        // Assert
        command.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void DeleteBrandCommand_ImplementsIRequest()
    {
        // Arrange
        var command = new DeleteBrandCommand(Guid.NewGuid());

        // Assert
        command.Should().BeAssignableTo<MediatR.IRequest>();
    }

    [Fact]
    public void DeleteBrandCommand_IsRecord()
    {
        // Arrange & Act
        var command = new DeleteBrandCommand(Guid.NewGuid());

        // Assert
        command.GetType().IsValueType.Should().BeFalse();
        command.GetType().IsClass.Should().BeTrue();
        command.GetType().IsSealed.Should().BeTrue();
    }
    
    [Fact]
    public void DeleteBrandCommand_WithSameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command1 = new DeleteBrandCommand(id);
        var command2 = new DeleteBrandCommand(id);

        // Act & Assert
        command1.Should().Be(command2);
        (command1 == command2).Should().BeTrue();
        command1.Equals(command2).Should().BeTrue();
    }
    
    [Fact]
    public void DeleteBrandCommand_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var command1 = new DeleteBrandCommand(Guid.NewGuid());
        var command2 = new DeleteBrandCommand(Guid.NewGuid());

        // Act & Assert
        command1.Should().NotBe(command2);
        (command1 != command2).Should().BeTrue();
        command1.Equals(command2).Should().BeFalse();
    }
    
    [Fact]
    public void DeleteBrandCommand_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new DeleteBrandCommand(id);

        // Act
        var result = command.ToString();

        // Assert
        result.Should().Be($"DeleteBrandCommand {{ Id = {id} }}");
    }
}
