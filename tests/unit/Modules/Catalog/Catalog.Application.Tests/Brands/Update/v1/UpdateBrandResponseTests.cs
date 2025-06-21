using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Brands.Update.v1;

public class UpdateBrandResponseTests
{
    [Fact]
    public void Constructor_WithId_SetsIdProperty()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = new UpdateBrandResponse(id);

        // Assert
        response.Id.Should().Be(id);
    }

    [Fact]
    public void Constructor_WithNullId_SetsIdToNull()
    {
        // Act
        var response = new UpdateBrandResponse(null);

        // Assert
        response.Id.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithDefaultGuid_SetsIdToEmptyGuid()
    {
        // Arrange
        var defaultGuid = Guid.Empty;

        // Act
        var response = new UpdateBrandResponse(defaultGuid);

        // Assert
        response.Id.Should().Be(defaultGuid);
    }
    
    [Fact]
    public void UpdateBrandResponse_IsRecord()
    {
        // Arrange & Act
        var response = new UpdateBrandResponse(Guid.NewGuid());

        // Assert
        response.GetType().IsValueType.Should().BeFalse();
        response.GetType().IsClass.Should().BeTrue();
        response.GetType().IsSealed.Should().BeTrue();
    }
    
    [Fact]
    public void UpdateBrandResponse_WithSameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response1 = new UpdateBrandResponse(id);
        var response2 = new UpdateBrandResponse(id);

        // Act & Assert
        response1.Should().Be(response2);
        (response1 == response2).Should().BeTrue();
        response1.Equals(response2).Should().BeTrue();
    }
    
    [Fact]
    public void UpdateBrandResponse_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var response1 = new UpdateBrandResponse(Guid.NewGuid());
        var response2 = new UpdateBrandResponse(Guid.NewGuid());

        // Act & Assert
        response1.Should().NotBe(response2);
        (response1 != response2).Should().BeTrue();
        response1.Equals(response2).Should().BeFalse();
    }
    
    [Fact]
    public void UpdateBrandResponse_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = new UpdateBrandResponse(id);

        // Act
        var result = response.ToString();

        // Assert
        result.Should().Be($"UpdateBrandResponse {{ Id = {id} }}");
    }
}
