using System;
using FluentAssertions;
using Xunit;
using FSH.Starter.WebApi.Catalog.Application.Brands.Get.v1;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Products.Get.v1;

public class ProductResponseTests
{
    private readonly Guid _testId = Guid.NewGuid();
    private const string TestName = "Test Product";
    private const string TestDescription = "Test Description";
    private const decimal TestPrice = 9.99m;
    private readonly BrandResponse? _testBrand = new(Guid.NewGuid(), "Test Brand", "Brand Description");

    [Fact]
    public void Constructor_WithValidParameters_SetsProperties()
    {
        // Act
        var response = new ProductResponse(_testId, TestName, TestDescription, TestPrice, _testBrand);

        // Assert
        response.Id.Should().Be(_testId);
        response.Name.Should().Be(TestName);
        response.Description.Should().Be(TestDescription);
        response.Price.Should().Be(TestPrice);
        response.Brand.Should().Be(_testBrand);
    }

    [Fact]
    public void Constructor_WithNullId_SetsIdToNull()
    {
        // Act
        var response = new ProductResponse(null, TestName, TestDescription, TestPrice, _testBrand);

        // Assert
        response.Id.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ProductResponse(_testId, null!, TestDescription, TestPrice, _testBrand));
    }

    [Fact]
    public void Constructor_WithNullDescription_SetsDescriptionToNull()
    {
        // Act
        var response = new ProductResponse(_testId, TestName, null, TestPrice, _testBrand);

        // Assert
        response.Description.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullBrand_SetsBrandToNull()
    {
        // Act
        var response = new ProductResponse(_testId, TestName, TestDescription, TestPrice, null);

        // Assert
        response.Brand.Should().BeNull();
    }

    [Fact]
    public void ProductResponse_IsRecord()
    {
        // Arrange & Act
        var response = new ProductResponse(_testId, TestName, TestDescription, TestPrice, _testBrand);

        // Assert
        response.GetType().IsValueType.Should().BeFalse();
        response.GetType().IsClass.Should().BeTrue();
        response.GetType().IsSealed.Should().BeTrue();
    }

    [Fact]
    public void ProductResponse_WithSameValues_AreEqual()
    {
        // Arrange
        var response1 = new ProductResponse(_testId, TestName, TestDescription, TestPrice, _testBrand);
        var response2 = new ProductResponse(_testId, TestName, TestDescription, TestPrice, _testBrand);

        // Act & Assert
        response1.Should().Be(response2);
        (response1 == response2).Should().BeTrue();
        response1.Equals(response2).Should().BeTrue();
    }
    
    [Fact]
    public void ProductResponse_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var response1 = new ProductResponse(_testId, "Product 1", "Desc 1", 10.00m, _testBrand);
        var response2 = new ProductResponse(Guid.NewGuid(), "Product 2", "Desc 2", 20.00m, null);

        // Act & Assert
        response1.Should().NotBe(response2);
        (response1 != response2).Should().BeTrue();
        response1.Equals(response2).Should().BeFalse();
    }
    
    [Fact]
    public void ProductResponse_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var response = new ProductResponse(_testId, TestName, TestDescription, TestPrice, _testBrand);

        // Act
        var result = response.ToString();

        // Assert
        result.Should().Contain(TestName);
        result.Should().Contain(TestPrice.ToString());
        result.Should().Contain(_testId.ToString());
    }
    
    [Fact]
    public void ProductResponse_WithNullDescription_ToStringHandlesNull()
    {
        // Arrange
        var response = new ProductResponse(_testId, TestName, null, TestPrice, null);

        // Act
        var result = response.ToString();

        // Assert
        result.Should().Contain("Description = ");
        result.Should().Contain("Brand = ");
    }
    
    [Fact]
    public void ProductResponse_WithZeroPrice_ToStringIncludesZero()
    {
        // Arrange
        var response = new ProductResponse(_testId, TestName, TestDescription, 0, _testBrand);

        // Act
        var result = response.ToString();

        // Assert
        result.Should().Contain("Price = 0");
    }
}
