using FluentAssertions;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Brands.Get.v1;

public class BrandResponseTests
{
    [Fact]
    public void Constructor_WithValidParameters_SetsProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Brand";
        var description = "Test Description";

        // Act
        var response = new BrandResponse(id, name, description);

        // Assert
        response.Id.Should().Be(id);
        response.Name.Should().Be(name);
        response.Description.Should().Be(description);
    }

    [Fact]
    public void Constructor_WithNullId_SetsIdToNull()
    {
        // Act
        var response = new BrandResponse(null, "Name", "Description");

        // Assert
        response.Id.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullDescription_SetsDescriptionToNull()
    {
        // Act
        var response = new BrandResponse(Guid.NewGuid(), "Name", null);

        // Assert
        response.Description.Should().BeNull();
    }

    [Fact]
    public void BrandResponse_IsRecord()
    {
        // Arrange & Act
        var response = new BrandResponse(Guid.NewGuid(), "Name", "Description");

        // Assert
        response.GetType().IsValueType.Should().BeFalse();
        response.GetType().IsClass.Should().BeTrue();
        response.GetType().IsSealed.Should().BeTrue();
    }
    
    [Fact]
    public void BrandResponse_WithSameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response1 = new BrandResponse(id, "Name", "Description");
        var response2 = new BrandResponse(id, "Name", "Description");

        // Act & Assert
        response1.Should().Be(response2);
        (response1 == response2).Should().BeTrue();
        response1.Equals(response2).Should().BeTrue();
    }
    
    [Fact]
    public void BrandResponse_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var response1 = new BrandResponse(Guid.NewGuid(), "Name 1", "Description 1");
        var response2 = new BrandResponse(Guid.NewGuid(), "Name 2", "Description 2");

        // Act & Assert
        response1.Should().NotBe(response2);
        (response1 != response2).Should().BeTrue();
        response1.Equals(response2).Should().BeFalse();
    }
    
    [Fact]
    public void BrandResponse_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = new BrandResponse(id, "Test Brand", "Test Description");

        // Act
        var result = response.ToString();

        // Assert
        result.Should().Be($"BrandResponse {{ Id = {id}, Name = Test Brand, Description = Test Description }}");
    }
    
    [Fact]
    public void BrandResponse_WithNullDescription_ToStringOmitsDescription()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = new BrandResponse(id, "Test Brand", null);

        // Act
        var result = response.ToString();

        // Assert
        result.Should().Be($"BrandResponse {{ Id = {id}, Name = Test Brand, Description =  }}");
    }
}
