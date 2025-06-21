using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Brands.Create.v1;

public class CreateBrandResponseTests
{
    [Fact]
    public void Constructor_WithId_SetsIdProperty()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = new CreateBrandResponse(id);

        // Assert
        response.Id.Should().Be(id);
    }

    [Fact]
    public void Constructor_WithNullId_SetsIdToNull()
    {
        // Act
        var response = new CreateBrandResponse(null);

        // Assert
        response.Id.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithDefaultGuid_SetsIdToEmptyGuid()
    {
        // Arrange
        var defaultGuid = Guid.Empty;

        // Act
        var response = new CreateBrandResponse(defaultGuid);

        // Assert
        response.Id.Should().Be(defaultGuid);
    }

    
    [Fact]
    public void CreateBrandResponse_IsRecord()
    {
        // Arrange & Act
        var response = new CreateBrandResponse(Guid.NewGuid());

        // Assert
        response.GetType().IsValueType.Should().BeFalse();
        response.GetType().IsClass.Should().BeTrue();
        response.GetType().IsSealed.Should().BeTrue();
    }
    
    [Fact]
    public void CreateBrandResponse_WithSameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response1 = new CreateBrandResponse(id);
        var response2 = new CreateBrandResponse(id);

        // Act & Assert
        response1.Should().Be(response2);
        (response1 == response2).Should().BeTrue();
        response1.Equals(response2).Should().BeTrue();
    }
    
    [Fact]
    public void CreateBrandResponse_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var response1 = new CreateBrandResponse(Guid.NewGuid());
        var response2 = new CreateBrandResponse(Guid.NewGuid());

        // Act & Assert
        response1.Should().NotBe(response2);
        (response1 != response2).Should().BeTrue();
        response1.Equals(response2).Should().BeFalse();
    }
    
    [Fact]
    public void CreateBrandResponse_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = new CreateBrandResponse(id);

        // Act
        var result = response.ToString();

        // Assert
        result.Should().Be($"CreateBrandResponse {{ Id = {id} }}");
    }
}
