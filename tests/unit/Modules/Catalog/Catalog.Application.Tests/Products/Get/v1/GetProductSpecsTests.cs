using System;
using System.Linq;
using Ardalis.Specification;
using FSH.Starter.WebApi.Catalog.Domain;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Products.Get.v1;

public class GetProductSpecsTests
{
    private readonly Guid _testId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidId_SetsUpSpecificationCorrectly()
    {
        // Act
        var spec = new GetProductSpecs(_testId);

        // Assert
        spec.Should().NotBeNull();
        spec.Query.Should().NotBeNull();
        
        // Verify the where condition
        var whereExpressions = spec.Query.WhereExpressions;
        whereExpressions.Should().HaveCount(1);
        var whereExpression = whereExpressions.First();
        
        // Create a sample product and test the filter
        var product = new Product("Test Product", "Description", 9.99m, null) { Id = _testId };
        var filter = whereExpression.FilterFunc.Compile();
        filter(product).Should().BeTrue();
        
        // Test with different ID
        var otherProduct = new Product("Other Product", "Other Desc", 19.99m, null) { Id = Guid.NewGuid() };
        filter(otherProduct).Should().BeFalse();
        
        // Verify the include
        var includeExpressions = spec.Query.IncludeExpressions;
        includeExpressions.Should().HaveCount(1);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ThrowsNoException()
    {
        // Act
        var spec = new GetProductSpecs(Guid.Empty);

        // Assert - No exception should be thrown
        spec.Should().NotBeNull();
        
        // The filter should still work, just won't match any product
        var whereExpressions = spec.Query.WhereExpressions;
        whereExpressions.Should().HaveCount(1);
        var whereExpression = whereExpressions.First();
        var filter = whereExpression.FilterFunc.Compile();
        
        // Only a product with empty GUID would match, which is unlikely but valid
        var product = new Product("Test", "Desc", 10m, null) { Id = Guid.Empty };
        filter(product).Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithSameId_CreatesEquivalentSpecs()
    {
        // Act
        var spec1 = new GetProductSpecs(_testId);
        var spec2 = new GetProductSpecs(_testId);

        // Assert - The specs should be functionally equivalent
        spec1.Should().NotBeSameAs(spec2);
        
        // Verify the where conditions are equivalent
        var filter1 = spec1.Query.WhereExpressions.First().FilterFunc.Compile();
        var filter2 = spec2.Query.WhereExpressions.First().FilterFunc.Compile();
        
        var product = new Product("Test", "Desc", 10m, null) { Id = _testId };
        var otherProduct = new Product("Other", "Other Desc", 20m, null) { Id = Guid.NewGuid() };
        
        filter1(product).Should().BeTrue();
        filter2(product).Should().BeTrue();
        filter1(otherProduct).Should().BeFalse();
        filter2(otherProduct).Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithDifferentIds_CreatesDifferentSpecs()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        // Act
        var spec1 = new GetProductSpecs(id1);
        var spec2 = new GetProductSpecs(id2);

        // Assert - The specs should filter for different IDs
        var filter1 = spec1.Query.WhereExpressions.First().FilterFunc.Compile();
        var filter2 = spec2.Query.WhereExpressions.First().FilterFunc.Compile();
        
        var product1 = new Product("Test 1", "Desc 1", 10m, null) { Id = id1 };
        var product2 = new Product("Test 2", "Desc 2", 20m, null) { Id = id2 };
        
        filter1(product1).Should().BeTrue();
        filter1(product2).Should().BeFalse();
        filter2(product1).Should().BeFalse();
        filter2(product2).Should().BeTrue();
    }

    [Fact]
    public void Constructor_IncludesBrandInQuery()
    {
        // Act
        var spec = new GetProductSpecs(_testId);

        // Assert - Should include Brand in the query
        var includeExpressions = spec.Query.IncludeExpressions;
        includeExpressions.Should().HaveCount(1);
        
        // The include should be for the Brand navigation property
        var includeString = includeExpressions.First().ToString();
        includeString.Should().Contain("Brand");
    }
}
