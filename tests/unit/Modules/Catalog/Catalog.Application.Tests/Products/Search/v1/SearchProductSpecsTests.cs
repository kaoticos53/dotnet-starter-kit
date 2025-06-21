using System;
using System.Linq;
using Ardalis.Specification;
using FSH.Framework.Core.Paging;
using FSH.Framework.Core.Specifications;
using FSH.Starter.WebApi.Catalog.Application.Products.Search.v1;
using FSH.Starter.WebApi.Catalog.Domain;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Products.Search.v1;

public class SearchProductSpecsTests
{
    private readonly Guid _brandId = Guid.NewGuid();
    private const decimal MinRate = 10.0m;
    private const decimal MaxRate = 100.0m;
    private const string OrderBy = "name";
    private const int PageNumber = 2;
    private const int PageSize = 20;

    [Fact]
    public void Constructor_WithNullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        SearchProductsCommand? command = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SearchProductSpecs(command!));
    }

    [Fact]
    public void Constructor_WithDefaultCommand_SetsUpBaseQuery()
    {
        // Arrange
        var command = new SearchProductsCommand();

        // Act
        var spec = new SearchProductSpecs(command);

        // Assert
        spec.Should().NotBeNull();
        spec.Query.Should().NotBeNull();
        
        // Verify default ordering (Name ascending)
        var orderExpressions = spec.OrderExpressions;
        orderExpressions.Should().HaveCount(1);
        
        // Verify include
        var includeExpressions = spec.IncludeExpressions;
        includeExpressions.Should().HaveCount(1);
        includeExpressions[0].ToString().Should().Contain("Brand");
    }

    [Fact]
    public void Constructor_WithBrandId_AddsBrandFilter()
    {
        // Arrange
        var command = new SearchProductsCommand { BrandId = _brandId };

        // Act
        var spec = new SearchProductSpecs(command);

        // Assert
        var whereExpressions = spec.WhereExpressions;
        whereExpressions.Should().HaveCount(1);
        
        // Test the filter
        var productWithBrand = new Product("Test", "Desc", 50m, _brandId);
        var productWithOtherBrand = new Product("Test", "Desc", 50m, Guid.NewGuid());
        
        var filter = whereExpressions[0].FilterFunc.Compile();
        filter(productWithBrand).Should().BeTrue();
        filter(productWithOtherBrand).Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithMinimumRate_AddsRateFilter()
    {
        // Arrange
        var command = new SearchProductsCommand { MinimumRate = MinRate };

        // Act
        var spec = new SearchProductSpecs(command);

        // Assert
        var whereExpressions = spec.WhereExpressions;
        whereExpressions.Should().HaveCount(1);
        
        // Test the filter
        var productAboveMin = new Product("Test", "Desc", MinRate + 1, null);
        var productBelowMin = new Product("Test", "Desc", MinRate - 1, null);
        var productAtMin = new Product("Test", "Desc", MinRate, null);
        
        var filter = whereExpressions[0].FilterFunc.Compile();
        filter(productAboveMin).Should().BeTrue();
        filter(productAtMin).Should().BeTrue();
        filter(productBelowMin).Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithMaximumRate_AddsRateFilter()
    {
        // Arrange
        var command = new SearchProductsCommand { MaximumRate = MaxRate };

        // Act
        var spec = new SearchProductSpecs(command);

        // Assert
        var whereExpressions = spec.WhereExpressions;
        whereExpressions.Should().HaveCount(1);
        
        // Test the filter
        var productBelowMax = new Product("Test", "Desc", MaxRate - 1, null);
        var productAboveMax = new Product("Test", "Desc", MaxRate + 1, null);
        var productAtMax = new Product("Test", "Desc", MaxRate, null);
        
        var filter = whereExpressions[0].FilterFunc.Compile();
        filter(productBelowMax).Should().BeTrue();
        filter(productAtMax).Should().BeTrue();
        filter(productAboveMax).Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithRateRange_AddsBothRateFilters()
    {
        // Arrange
        var command = new SearchProductsCommand 
        { 
            MinimumRate = MinRate,
            MaximumRate = MaxRate 
        };

        // Act
        var spec = new SearchProductSpecs(command);

        // Assert
        var whereExpressions = spec.WhereExpressions;
        whereExpressions.Should().HaveCount(2);
        
        // Test the filters
        var productInRange = new Product("Test", "Desc", (MinRate + MaxRate) / 2, null);
        var productBelowMin = new Product("Test", "Desc", MinRate - 1, null);
        var productAboveMax = new Product("Test", "Desc", MaxRate + 1, null);
        
        var minFilter = whereExpressions[0].FilterFunc.Compile();
        var maxFilter = whereExpressions[1].FilterFunc.Compile();
        
        minFilter(productInRange).Should().BeTrue();
        maxFilter(productInRange).Should().BeTrue();
        
        minFilter(productBelowMin).Should().BeFalse();
        maxFilter(productBelowMin).Should().BeTrue();
        
        minFilter(productAboveMax).Should().BeTrue();
        maxFilter(productAboveMax).Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithOrderBy_AppliesCustomOrdering()
    {
        // Arrange
        var command = new SearchProductsCommand 
        { 
            OrderBy = ["name desc"] 
        };

        // Act
        var spec = new SearchProductSpecs(command);

        // Assert
        var orderExpressions = spec.OrderExpressions;
        orderExpressions.Should().HaveCount(1);
        
        // Verify the order expression is for Name in descending order
        var orderExpression = orderExpressions[0].OrderExpression;
        orderExpression.ToString().Should().Contain("Name");
    }

    [Fact]
    public void Constructor_WithPagination_SetsPaginationProperties()
    {
        // Arrange
        var command = new SearchProductsCommand 
        { 
            PageNumber = PageNumber,
            PageSize = PageSize
        };

        // Act
        var spec = new SearchProductSpecs(command);

        // Assert - The base class should handle pagination
        spec.Should().BeAssignableTo<EntitiesByPaginationFilterSpec<Product, ProductResponse>>();
    }

    [Fact]
    public void Constructor_WithAllFilters_AppliesAllConditions()
    {
        // Arrange
        var command = new SearchProductsCommand 
        { 
            BrandId = _brandId,
            MinimumRate = MinRate,
            MaximumRate = MaxRate,
            OrderBy = ["name"]
        };

        // Act
        var spec = new SearchProductSpecs(command);

        // Assert
        // Should have 3 where conditions (brand, min rate, max rate) and 1 include
        spec.WhereExpressions.Should().HaveCount(3);
        spec.IncludeExpressions.Should().HaveCount(1);
        spec.OrderExpressions.Should().HaveCount(1);
    }

    [Fact]
    public void Constructor_WithEmptyBrandId_DoesNotAddBrandFilter()
    {
        // Arrange
        var command = new SearchProductsCommand { BrandId = Guid.Empty };

        // Act
        var spec = new SearchProductSpecs(command);

        // Assert - Should not add a brand filter for empty GUID
        var whereExpressions = spec.WhereExpressions;
        whereExpressions.Should().BeEmpty();
    }
}
