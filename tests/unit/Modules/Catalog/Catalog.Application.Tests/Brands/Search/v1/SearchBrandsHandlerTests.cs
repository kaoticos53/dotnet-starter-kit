using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FSH.Framework.Core.Paging;
using FSH.Framework.Core.Persistence;
using FSH.Starter.WebApi.Catalog.Application.Brands.Get.v1;
using FSH.Starter.WebApi.Catalog.Domain;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Brands.Search.v1;

public class SearchBrandsHandlerTests
{
    private readonly Mock<IReadRepository<Brand>> _repositoryMock;
    private readonly SearchBrandsHandler _handler;
    private readonly List<Brand> _testBrands;

    public SearchBrandsHandlerTests()
    {
        _repositoryMock = new Mock<IReadRepository<Brand>>();
        
        var serviceProvider = new ServiceCollection()
            .AddKeyedScoped("catalog:brands", sp => _repositoryMock.Object)
            .BuildServiceProvider();
            
        _handler = new SearchBrandsHandler(
            serviceProvider.GetRequiredKeyedService<IReadRepository<Brand>>("catalog:brands"));

        // Setup test data
        _testBrands = new List<Brand>
        {
            Brand.Create("Brand 1", "Description 1"),
            Brand.Create("Brand 2", "Description 2"),
            Brand.Create("Another Brand", "Another Description")
        };
    }

    [Fact]
    public async Task Handle_WithValidRequest_ReturnsPagedListOfBrands()
    {
        // Arrange
        var request = new SearchBrandsCommand { PageNumber = 1, PageSize = 10 };
        var pagedBrands = new PagedList<Brand>(_testBrands, 1, 10, _testBrands.Count);
        
        _repositoryMock
            .Setup(r => r.ListAsync(It.IsAny<SearchBrandSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testBrands);
            
        _repositoryMock
            .Setup(r => r.CountAsync(It.IsAny<SearchBrandSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testBrands.Count);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(_testBrands.Count);
        result.TotalCount.Should().Be(_testBrands.Count);
        result.PageNumber.Should().Be(request.PageNumber);
        result.PageSize.Should().Be(request.PageSize);
        
        _repositoryMock.Verify(
            r => r.ListAsync(It.IsAny<SearchBrandSpecs>(), It.IsAny<CancellationToken>()),
            Times.Once);
            
        _repositoryMock.Verify(
            r => r.CountAsync(It.IsAny<SearchBrandSpecs>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var request = new SearchBrandsCommand { PageNumber = 2, PageSize = 1 };
        var pagedBrands = new PagedList<Brand>(_testBrands.Take(1).ToList(), 2, 1, _testBrands.Count);
        
        _repositoryMock
            .Setup(r => r.ListAsync(It.IsAny<SearchBrandSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedBrands.Items);
            
        _repositoryMock
            .Setup(r => r.CountAsync(It.IsAny<SearchBrandSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testBrands.Count);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(_testBrands.Count);
        result.PageNumber.Should().Be(request.PageNumber);
        result.PageSize.Should().Be(request.PageSize);
    }

    [Fact]
    public async Task Handle_WithKeyword_FiltersBrands()
    {
        // Arrange
        var keyword = "Another";
        var request = new SearchBrandsCommand { Keyword = keyword };
        var filteredBrands = _testBrands.Where(b => b.Name.Contains(keyword)).ToList();
        
        _repositoryMock
            .Setup(r => r.ListAsync(It.IsAny<SearchBrandSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(filteredBrands);
            
        _repositoryMock
            .Setup(r => r.CountAsync(It.IsAny<SearchBrandSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(filteredBrands.Count);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Contain(keyword);
    }

    [Fact]
    public async Task Handle_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        SearchBrandsCommand? request = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _handler.Handle(request!, CancellationToken.None));
            
        _repositoryMock.Verify(
            r => r.ListAsync(It.IsAny<SearchBrandSpecs>(), It.IsAny<CancellationToken>()),
            Times.Never);
            
        _repositoryMock.Verify(
            r => r.CountAsync(It.IsAny<SearchBrandSpecs>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var request = new SearchBrandsCommand();
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(request, cancellationToken));
            
        _repositoryMock.Verify(
            r => r.ListAsync(It.IsAny<SearchBrandSpecs>(), It.IsAny<CancellationToken>()),
            Times.Never);
            
        _repositoryMock.Verify(
            r => r.CountAsync(It.IsAny<SearchBrandSpecs>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ReturnsEmptyPagedList()
    {
        // Arrange
        var request = new SearchBrandsCommand { Keyword = "Nonexistent" };
        var emptyList = new List<Brand>();
        
        _repositoryMock
            .Setup(r => r.ListAsync(It.IsAny<SearchBrandSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);
            
        _repositoryMock
            .Setup(r => r.CountAsync(It.IsAny<SearchBrandSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(request.PageNumber);
        result.PageSize.Should().Be(request.PageSize);
    }

    [Fact]
    public async Task Handle_WithCustomOrdering_AppliesOrdering()
    {
        // Arrange
        var request = new SearchBrandsCommand 
        { 
            OrderBy = ["name desc"] 
        };
        
        var orderedBrands = _testBrands
            .OrderByDescending(b => b.Name)
            .ToList();
            
        _repositoryMock
            .Setup(r => r.ListAsync(It.IsAny<SearchBrandSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderedBrands);
            
        _repositoryMock
            .Setup(r => r.CountAsync(It.IsAny<SearchBrandSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderedBrands.Count);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Items[0].Name.Should().Be(orderedBrands[0].Name);
        result.Items[1].Name.Should().Be(orderedBrands[1].Name);
        result.Items[2].Name.Should().Be(orderedBrands[2].Name);
    }
}
