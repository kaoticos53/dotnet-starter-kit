using System;
using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Products.Delete.v1;

public class DeleteProductCommandTests
{
    private readonly Guid _testId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidId_SetsIdProperty()
    {
        // Act
        var command = new DeleteProductCommand(_testId);

        // Assert
        command.Id.Should().Be(_testId);
    }


    [Fact]
    public void Constructor_WithEmptyGuid_ThrowsNoException()
    {
        // Act
        var command = new DeleteProductCommand(Guid.Empty);

        // Assert
        command.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void DeleteProductCommand_ImplementsIRequest()
    {
        // Arrange & Act
        var command = new DeleteProductCommand(_testId);

        // Assert
        command.Should().BeAssignableTo<MediatR.IRequest>();
    }

    [Fact]
    public void DeleteProductCommand_IsRecord()
    {
        // Arrange & Act
        var command = new DeleteProductCommand(_testId);

        // Assert
        command.GetType().IsValueType.Should().BeFalse();
        command.GetType().IsClass.Should().BeTrue();
        command.GetType().IsSealed.Should().BeTrue();
    }

    [Fact]
    public void DeleteProductCommand_WithSameValues_AreEqual()
    {
        // Arrange
        var command1 = new DeleteProductCommand(_testId);
        var command2 = new DeleteProductCommand(_testId);

        // Act & Assert
        command1.Should().Be(command2);
        (command1 == command2).Should().BeTrue();
        command1.Equals(command2).Should().BeTrue();
    }
    
    [Fact]
    public void DeleteProductCommand_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var command1 = new DeleteProductCommand(_testId);
        var command2 = new DeleteProductCommand(Guid.NewGuid());

        // Act & Assert
        command1.Should().NotBe(command2);
        (command1 != command2).Should().BeTrue();
        command1.Equals(command2).Should().BeFalse();
    }
    
    [Fact]
    public void DeleteProductCommand_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var command = new DeleteProductCommand(_testId);

        // Act
        var result = command.ToString();

        // Assert
        result.Should().Be($"DeleteProductCommand {{ Id = {_testId} }}");
    }
}
