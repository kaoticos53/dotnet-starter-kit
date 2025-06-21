using System;
using FluentAssertions;
using Xunit;
using FSH.Framework.Core.Paging;
using FSH.Starter.WebApi.Catalog.Application.Brands.Get.v1;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Brands.Search.v1;

public class SearchBrandsCommandTests
{
    [Fact]
    public void Constructor_WithDefaultValues_SetsProperties()
    {
        // Act
        var command = new SearchBrandsCommand();

        // Assert
        command.PageNumber.Should().Be(1);
        command.PageSize.Should().Be(10);
        command.Keyword.Should().BeNull();
        command.Name.Should().BeNull();
        command.Description.Should().BeNull();
    }

    [Theory]
    [InlineData(1, 10, "test", "test name", "test description")]
    [InlineData(2, 20, "search", null, null)]
    [InlineData(3, 5, null, "name only", null)]
    [InlineData(4, 15, null, null, "description only")]
    public void Properties_WithValues_CanBeSetAndGet(
        int pageNumber, 
        int pageSize, 
        string? keyword,
        string? name,
        string? description)
    {
        // Arrange
        var command = new SearchBrandsCommand
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Keyword = keyword,
            Name = name,
            Description = description
        };

        // Assert
        command.PageNumber.Should().Be(pageNumber);
        command.PageSize.Should().Be(pageSize);
        command.Keyword.Should().Be(keyword);
        command.Name.Should().Be(name);
        command.Description.Should().Be(description);
    }

    [Fact]
    public void SearchBrandsCommand_ImplementsIRequestOfPagedListOfBrandResponse()
    {
        // Arrange & Act
        var command = new SearchBrandsCommand();

        // Assert
        command.Should().BeAssignableTo<MediatR.IRequest<PagedList<BrandResponse>>>();
    }

    [Fact]
    public void SearchBrandsCommand_InheritsFromPaginationFilter()
    {
        // Arrange & Act
        var command = new SearchBrandsCommand();

        // Assert
        command.Should().BeAssignableTo<PaginationFilter>();
    }

    [Fact]
    public void SearchBrandsCommand_WithNegativePageNumber_SetsToDefault()
    {
        // Arrange
        var command = new SearchBrandsCommand { PageNumber = -1 };

        // Assert
        command.PageNumber.Should().Be(1);
    }

    [Fact]
    public void SearchBrandsCommand_WithZeroPageSize_SetsToDefault()
    {
        // Arrange
        var command = new SearchBrandsCommand { PageSize = 0 };

        // Assert
        command.PageSize.Should().Be(10);
    }

    [Fact]
    public void SearchBrandsCommand_WithEmptyString_ConvertsToNull()
    {
        // Arrange
        var command = new SearchBrandsCommand
        {
            Keyword = "",
            Name = "",
            Description = ""
        };

        // Assert
        command.Keyword.Should().BeNull();
        command.Name.Should().BeNull();
        command.Description.Should().BeNull();
    }

    [Fact]
    public void SearchBrandsCommand_WithWhitespace_ConvertsToNull()
    {
        // Arrange
        var command = new SearchBrandsCommand
        {
            Keyword = "   ",
            Name = "   ",
            Description = "   "
        };

        // Assert
        command.Keyword.Should().BeNull();
        command.Name.Should().BeNull();
        command.Description.Should().BeNull();
    }
}
