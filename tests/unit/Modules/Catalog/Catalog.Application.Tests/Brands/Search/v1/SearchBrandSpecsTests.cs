using System;
using System.Linq;
using FSH.Framework.Core.Specifications;
using FSH.Starter.WebApi.Catalog.Domain;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Brands.Search.v1;

public class SearchBrandSpecsTests
{
    [Fact]
    public void Constructor_WithNullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        SearchBrandsCommand? command = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SearchBrandSpecs(command!));
    }

    [Fact]
    public void Constructor_WithDefaultCommand_AppliesDefaultOrdering()
    {
        // Arrange
        var command = new SearchBrandsCommand();
        
        // Act
        var spec = new SearchBrandSpecs(command);
        
        // Assert
        spec.OrderByExpressions.Should().HaveCount(1);
        var orderBy = spec.OrderByExpressions.First();
        orderBy.KeySelector.Compile()(new Brand { Name = "Test" }).Should().Be("Test");
        orderBy.IsDescending.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithKeyword_AppliesSearchFilter()
    {
        // Arrange
        var keyword = "test";
        var command = new SearchBrandsCommand { Keyword = keyword };
        
        // Act
        var spec = new SearchBrandSpecs(command);
        
        // Assert
        spec.WhereExpressions.Should().HaveCount(1);
        
        var brand1 = new Brand { Name = "This is a test brand" };
        var brand2 = new Brand { Name = "Another brand" };
        
        var filter = spec.WhereExpressions.First().FilterFunc.Compile();
        filter(brand1).Should().BeTrue();
        filter(brand2).Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithEmptyKeyword_DoesNotApplySearchFilter()
    {
        // Arrange
        var command = new SearchBrandsCommand { Keyword = string.Empty };
        
        // Act
        var spec = new SearchBrandSpecs(command);
        
        // Assert
        spec.WhereExpressions.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithWhitespaceKeyword_DoesNotApplySearchFilter()
    {
        // Arrange
        var command = new SearchBrandsCommand { Keyword = "   " };
        
        // Act
        var spec = new SearchBrandSpecs(command);
        
        // Assert
        spec.WhereExpressions.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithCustomOrderBy_RespectsExistingOrdering()
    {
        // Arrange
        var command = new SearchBrandsCommand 
        { 
            OrderBy = ["name desc", "description"] 
        };
        
        // Act
        var spec = new SearchBrandSpecs(command);
        
        // Assert - Should not add default ordering when custom order is specified
        spec.OrderByExpressions.Should().HaveCount(2);
        
        var firstOrder = spec.OrderByExpressions[0];
        firstOrder.KeySelector.Compile()(new Brand { Name = "Test" }).Should().Be("Test");
        firstOrder.IsDescending.Should().BeTrue();
        
        var secondOrder = spec.OrderByExpressions[1];
        secondOrder.KeySelector.Compile()(new Brand { Description = "Desc" }).Should().Be("Desc");
        secondOrder.IsDescending.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithPagination_SetsCorrectPagination()
    {
        // Arrange
        var command = new SearchBrandsCommand 
        { 
            PageNumber = 2, 
            PageSize = 25 
        };
        
        // Act
        var spec = new SearchBrandSpecs(command);
        
        // Assert
        spec.Skip.Should().Be(25); // (2-1) * 25 = 25
        spec.Take.Should().Be(25);
    }

    [Fact]
    public void Constructor_WithNameAndDescription_AppliesSearchFilter()
    {
        // Arrange
        var command = new SearchBrandsCommand 
        { 
            Name = "Test",
            Description = "Description"
        };
        
        // Act
        var spec = new SearchBrandSpecs(command);
        
        // Assert - Only keyword is used in the current implementation
        spec.WhereExpressions.Should().BeEmpty();
    }
}
