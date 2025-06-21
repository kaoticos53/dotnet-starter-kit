using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Products.Update.v1;

public class UpdateProductResponseTests
{
    [Fact]
    public void Constructor_WithId_SetsIdProperty()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = new UpdateProductResponse(id);

        // Assert
        response.Id.Should().Be(id);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_SetsIdToEmptyGuid()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var response = new UpdateProductResponse(emptyGuid);

        // Assert
        response.Id.Should().Be(emptyGuid);
    }
    
    [Fact]
    public void UpdateProductResponse_IsRecord()
    {
        // Arrange & Act
        var response = new UpdateProductResponse(Guid.NewGuid());

        // Assert
        response.GetType().IsValueType.Should().BeFalse();
        response.GetType().IsClass.Should().BeTrue();
        response.GetType().IsSealed.Should().BeTrue();
    }
    
    [Fact]
    public void UpdateProductResponse_WithSameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response1 = new UpdateProductResponse(id);
        var response2 = new UpdateProductResponse(id);

        // Act & Assert
        response1.Should().Be(response2);
        (response1 == response2).Should().BeTrue();
        response1.Equals(response2).Should().BeTrue();
    }
    
    [Fact]
    public void UpdateProductResponse_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var response1 = new UpdateProductResponse(Guid.NewGuid());
        var response2 = new UpdateProductResponse(Guid.NewGuid());

        // Act & Assert
        response1.Should().NotBe(response2);
        (response1 != response2).Should().BeTrue();
        response1.Equals(response2).Should().BeFalse();
    }
    
    [Fact]
    public void UpdateProductResponse_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = new UpdateProductResponse(id);

        // Act
        var result = response.ToString();

        // Assert
        result.Should().Be($"UpdateProductResponse {{ Id = {id} }}");
    }
}
