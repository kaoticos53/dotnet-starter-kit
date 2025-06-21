using System;
using FluentAssertions;
using FSH.Framework.Core.Paging;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Products.Search.v1;

public class SearchProductsCommandTests
{
    [Fact]
    public void Constructor_WithNoParameters_SetsDefaultValues()
    {
        // Act
        var command = new SearchProductsCommand();

        // Assert
        command.BrandId.Should().BeNull();
        command.MinimumRate.Should().BeNull();
        command.MaximumRate.Should().BeNull();
        command.PageNumber.Should().Be(1);
        command.PageSize.Should().Be(10);
        command.OrderBy.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithParameters_SetsProperties()
    {
        // Arrange
        var brandId = Guid.NewGuid();
        const decimal minRate = 10.5m;
        const decimal maxRate = 100.0m;
        const int pageNumber = 2;
        const int pageSize = 20;
        const string orderBy = "name";

        // Act
        var command = new SearchProductsCommand
        {
            BrandId = brandId,
            MinimumRate = minRate,
            MaximumRate = maxRate,
            PageNumber = pageNumber,
            PageSize = pageSize,
            OrderBy = [orderBy]
        };

        // Assert
        command.BrandId.Should().Be(brandId);
        command.MinimumRate.Should().Be(minRate);
        command.MaximumRate.Should().Be(maxRate);
        command.PageNumber.Should().Be(pageNumber);
        command.PageSize.Should().Be(pageSize);
        command.OrderBy.Should().ContainSingle(o => o == orderBy);
    }

    [Fact]
    public void SearchProductsCommand_ImplementsIRequestOfPagedListOfProductResponse()
    {
        // Arrange & Act
        var command = new SearchProductsCommand();

        // Assert
        command.Should().BeAssignableTo<MediatR.IRequest<PagedList<ProductResponse>>>();
    }

    [Fact]
    public void SearchProductsCommand_InheritsFromPaginationFilter()
    {
        // Arrange & Act
        var command = new SearchProductsCommand();

        // Assert
        command.Should().BeAssignableTo<PaginationFilter>();
    }

    [Fact]
    public void SearchProductsCommand_WithNegativePageNumber_ThrowsNoException()
    {
        // Act
        var command = new SearchProductsCommand { PageNumber = -1 };

        // Assert - No exception should be thrown as validation is handled by PaginationFilter
        command.PageNumber.Should().Be(-1);
    }

    [Fact]
    public void SearchProductsCommand_WithZeroPageSize_ThrowsNoException()
    {
        // Act
        var command = new SearchProductsCommand { PageSize = 0 };

        // Assert - No exception should be thrown as validation is handled by PaginationFilter
        command.PageSize.Should().Be(0);
    }

    [Fact]
    public void SearchProductsCommand_WithMultipleOrderBy_StoresAllValues()
    {
        // Arrange
        var orderBy = ["name", "price"];

        // Act
        var command = new SearchProductsCommand
        {
            OrderBy = orderBy
        };

        // Assert
        command.OrderBy.Should().BeEquivalentTo(orderBy);
    }

    [Fact]
    public void SearchProductsCommand_WithNullOrderBy_DoesNotThrow()
    {
        // Act
        var command = new SearchProductsCommand
        {
            OrderBy = null!
        };

        // Assert - Should not throw, OrderBy will be initialized to empty collection
        command.OrderBy.Should().NotBeNull();
        command.OrderBy.Should().BeEmpty();
    }
}
