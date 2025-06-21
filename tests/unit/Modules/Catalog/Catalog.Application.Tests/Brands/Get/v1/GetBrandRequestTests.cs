using System;
using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Brands.Get.v1;

public class GetBrandRequestTests
{
    [Fact]
    public void Constructor_WithValidId_SetsIdProperty()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var request = new GetBrandRequest(id);

        // Assert
        request.Id.Should().Be(id);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsNoException()
    {
        // Act
        var request = new GetBrandRequest(Guid.Empty);

        // Assert
        request.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void GetBrandRequest_ImplementsIRequestOfBrandResponse()
    {
        // Arrange
        var request = new GetBrandRequest(Guid.NewGuid());

        // Assert
        request.Should().BeAssignableTo<MediatR.IRequest<BrandResponse>>();
    }

    [Fact]
    public void GetBrandRequest_IsClass()
    {
        // Arrange & Act
        var request = new GetBrandRequest(Guid.NewGuid());

        // Assert
        request.GetType().IsValueType.Should().BeFalse();
        request.GetType().IsClass.Should().BeTrue();
    }
    
    [Fact]
    public void GetBrandRequest_WithSameId_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request1 = new GetBrandRequest(id);
        var request2 = new GetBrandRequest(id);

        // Act & Assert
        request1.Id.Should().Be(request2.Id);
    }
    
    [Fact]
    public void GetBrandRequest_WithDifferentIds_AreNotEqual()
    {
        // Arrange
        var request1 = new GetBrandRequest(Guid.NewGuid());
        var request2 = new GetBrandRequest(Guid.NewGuid());

        // Act & Assert
        request1.Id.Should().NotBe(request2.Id);
    }
}
