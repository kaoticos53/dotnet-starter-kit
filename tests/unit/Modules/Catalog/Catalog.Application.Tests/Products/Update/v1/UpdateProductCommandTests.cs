using System;
using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Products.Update.v1;

public class UpdateProductCommandTests
{
    private readonly Guid _testId = Guid.NewGuid();
    private const string TestName = "Updated Product";
    private const string TestDescription = "Updated Description";
    private const decimal TestPrice = 19.99m;
    private readonly Guid? _testBrandId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidParameters_SetsProperties()
    {
        // Act
        var command = new UpdateProductCommand(
            _testId,
            TestName,
            TestPrice,
            TestDescription,
            _testBrandId);

        // Assert
        command.Id.Should().Be(_testId);
        command.Name.Should().Be(TestName);
        command.Description.Should().Be(TestDescription);
        command.Price.Should().Be(TestPrice);
        command.BrandId.Should().Be(_testBrandId);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsNoException()
    {
        // Act
        var command = new UpdateProductCommand(Guid.Empty, TestName, TestPrice);

        // Assert
        command.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsNoException()
    {
        // Act
        var command = new UpdateProductCommand(_testId, null, TestPrice);

        // Assert
        command.Name.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullDescription_ThrowsNoException()
    {
        // Act
        var command = new UpdateProductCommand(_testId, TestName, TestPrice, null);

        // Assert
        command.Description.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullBrandId_ThrowsNoException()
    {
        // Act
        var command = new UpdateProductCommand(_testId, TestName, TestPrice, TestDescription, null);

        // Assert
        command.BrandId.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithZeroPrice_ThrowsNoException()
    {
        // Act
        var command = new UpdateProductCommand(_testId, TestName, 0);

        // Assert
        command.Price.Should().Be(0);
    }

    [Fact]
    public void UpdateProductCommand_ImplementsIRequestOfUpdateProductResponse()
    {
        // Arrange & Act
        var command = new UpdateProductCommand(_testId, TestName, TestPrice);

        // Assert
        command.Should().BeAssignableTo<MediatR.IRequest<UpdateProductResponse>>();
    }

    [Fact]
    public void UpdateProductCommand_IsRecord()
    {
        // Arrange & Act
        var command = new UpdateProductCommand(_testId, TestName, TestPrice);

        // Assert
        command.GetType().IsValueType.Should().BeFalse();
        command.GetType().IsClass.Should().BeTrue();
        command.GetType().IsSealed.Should().BeTrue();
    }

    [Fact]
    public void UpdateProductCommand_WithSameValues_AreEqual()
    {
        // Arrange
        var command1 = new UpdateProductCommand(_testId, TestName, TestPrice, TestDescription, _testBrandId);
        var command2 = new UpdateProductCommand(_testId, TestName, TestPrice, TestDescription, _testBrandId);

        // Act & Assert
        command1.Should().Be(command2);
        (command1 == command2).Should().BeTrue();
        command1.Equals(command2).Should().BeTrue();
    }
    
    [Fact]
    public void UpdateProductCommand_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var command1 = new UpdateProductCommand(_testId, "Name 1", 10.00m, "Desc 1", Guid.NewGuid());
        var command2 = new UpdateProductCommand(Guid.NewGuid(), "Name 2", 20.00m, "Desc 2", null);

        // Act & Assert
        command1.Should().NotBe(command2);
        (command1 != command2).Should().BeTrue();
        command1.Equals(command2).Should().BeFalse();
    }
    
    [Fact]
    public void UpdateProductCommand_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var command = new UpdateProductCommand(_testId, TestName, TestPrice, TestDescription, _testBrandId);

        // Act
        var result = command.ToString();

        // Assert
        result.Should().Be($"UpdateProductCommand {{ Id = {_testId}, Name = {TestName}, Price = {TestPrice}, Description = {TestDescription}, BrandId = {_testBrandId} }}");
    }

    [Fact]
    public void UpdateProductCommand_WithNullName_ToStringHandlesNull()
    {
        // Arrange
        var command = new UpdateProductCommand(_testId, null, TestPrice);

        // Act
        var result = command.ToString();

        // Assert
        result.Should().Be($"UpdateProductCommand {{ Id = {_testId}, Name = , Price = {TestPrice}, Description = , BrandId =  }}");
    }
}
