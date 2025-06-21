using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Products.Create.v1;

public class CreateProductResponseTests
{
    [Fact]
    public void Constructor_WithId_SetsIdProperty()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = new CreateProductResponse(id);

        // Assert
        response.Id.Should().Be(id);
    }

    [Fact]
    public void Constructor_WithNullId_SetsIdToNull()
    {
        // Act
        var response = new CreateProductResponse(null);

        // Assert
        response.Id.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithEmptyGuid_SetsIdToEmptyGuid()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var response = new CreateProductResponse(emptyGuid);

        // Assert
        response.Id.Should().Be(emptyGuid);
    }
    
    [Fact]
    public void CreateProductResponse_IsRecord()
    {
        // Arrange & Act
        var response = new CreateProductResponse(Guid.NewGuid());

        // Assert
        response.GetType().IsValueType.Should().BeFalse();
        response.GetType().IsClass.Should().BeTrue();
        response.GetType().IsSealed.Should().BeTrue();
    }
    
    [Fact]
    public void CreateProductResponse_WithSameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response1 = new CreateProductResponse(id);
        var response2 = new CreateProductResponse(id);

        // Act & Assert
        response1.Should().Be(response2);
        (response1 == response2).Should().BeTrue();
        response1.Equals(response2).Should().BeTrue();
    }
    
    [Fact]
    public void CreateProductResponse_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var response1 = new CreateProductResponse(Guid.NewGuid());
        var response2 = new CreateProductResponse(Guid.NewGuid());

        // Act & Assert
        response1.Should().NotBe(response2);
        (response1 != response2).Should().BeTrue();
        response1.Equals(response2).Should().BeFalse();
    }
    
    [Fact]
    public void CreateProductResponse_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = new CreateProductResponse(id);

        // Act
        var result = response.ToString();

        // Assert
        result.Should().Be($"CreateProductResponse {{ Id = {id} }}");
    }
    
    [Fact]
    public void CreateProductResponse_WithNullId_ToStringReturnsNull()
    {
        // Arrange
        var response = new CreateProductResponse(null);

        // Act
        var result = response.ToString();

        // Assert
        result.Should().Be("CreateProductResponse { Id =  }");
    }
}
