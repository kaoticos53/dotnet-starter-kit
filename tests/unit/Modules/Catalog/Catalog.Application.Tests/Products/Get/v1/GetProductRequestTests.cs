using System;
using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Products.Get.v1;

public class GetProductRequestTests
{
    private readonly Guid _testId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidId_SetsIdProperty()
    {
        // Act
        var request = new GetProductRequest(_testId);

        // Assert
        request.Id.Should().Be(_testId);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ThrowsNoException()
    {
        // Act
        var request = new GetProductRequest(Guid.Empty);

        // Assert
        request.Id.Should().Be(Guid.Empty);
    }
    [Fact]
    public void GetProductRequest_ImplementsIRequestOfProductResponse()
    {
        // Arrange & Act
        var request = new GetProductRequest(_testId);

        // Assert
        request.Should().BeAssignableTo<MediatR.IRequest<ProductResponse>>();
    }
    [Fact]
    public void GetProductRequest_WithSameValues_AreEqual()
    {
        // Arrange
        var request1 = new GetProductRequest(_testId);
        var request2 = new GetProductRequest(_testId);

        // Act & Assert
        request1.Id.Should().Be(request2.Id);
    }
    
    [Fact]
    public void GetProductRequest_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var request1 = new GetProductRequest(_testId);
        var request2 = new GetProductRequest(Guid.NewGuid());

        // Act & Assert
        request1.Id.Should().NotBe(request2.Id);
    }
    
    [Fact]
    public void GetProductRequest_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var request = new GetProductRequest(_testId);

        // Act
        var result = request.ToString();

        // Assert - Since it's a class, it will return the type name by default
        result.Should().Be($"{request.GetType().Name} Id = {_testId} ...");
    }
}
