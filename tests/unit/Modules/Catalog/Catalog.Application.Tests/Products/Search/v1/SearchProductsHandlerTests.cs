using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FSH.Framework.Core.Paging;
using FSH.Framework.Core.Persistence;
using FSH.Starter.WebApi.Catalog.Application.Products.Get.v1;
using FSH.Starter.WebApi.Catalog.Application.Products.Search.v1;
using FSH.Starter.WebApi.Catalog.Domain;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Products.Search.v1;

public class SearchProductsHandlerTests
{
    private readonly Mock<IReadRepository<Product>> _repositoryMock;
    private readonly SearchProductsHandler _handler;
    private readonly Guid _brandId = Guid.NewGuid();
    private const decimal MinRate = 10.0m;
    private const decimal MaxRate = 100.0m;
    private const int PageNumber = 2;
    private const int PageSize = 20;
    private const int TotalCount = 50;

    public SearchProductsHandlerTests()
    {
        _repositoryMock = new Mock<IReadRepository<Product>>();
        
        var serviceProvider = new ServiceCollection()
            .AddKeyedScoped("catalog:products", sp => _repositoryMock.Object)
            .BuildServiceProvider();
            
        _handler = new SearchProductsHandler(
            serviceProvider.GetRequiredKeyedService<IReadRepository<Product>>("catalog:products"));
    }

    [Fact]
    public async Task Handle_WithValidRequest_ReturnsPagedListOfProducts()
    {
        // Arrange
        var command = new SearchProductsCommand
        {
            BrandId = _brandId,
            MinimumRate = MinRate,
            MaximumRate = MaxRate,
            PageNumber = PageNumber,
            PageSize = PageSize
        };

        var products = new List<Product>
        {
            new("Product 1", "Desc 1", 50m, _brandId),
            new("Product 2", "Desc 2", 75m, _brandId)
        };

        _repositoryMock
            .Setup(r => r.ListAsync(It.IsAny<SearchProductSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
            
        _repositoryMock
            .Setup(r => r.CountAsync(It.IsAny<SearchProductSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TotalCount);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<PagedList<ProductResponse>>();
        result.Items.Should().HaveCount(products.Count);
        result.PageNumber.Should().Be(PageNumber);
        result.PageSize.Should().Be(PageSize);
        result.TotalCount.Should().Be(TotalCount);
        
        _repositoryMock.Verify(
            r => r.ListAsync(It.Is<SearchProductSpecs>(s => 
                s.BrandId == _brandId && 
                s.MinimumRate == MinRate && 
                s.MaximumRate == MaxRate &&
                s.PageNumber == PageNumber &&
                s.PageSize == PageSize), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        SearchProductsCommand? command = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _handler.Handle(command!, CancellationToken.None));
            
        _repositoryMock.Verify(
            r => r.ListAsync(It.IsAny<SearchProductSpecs>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsEmptyList_ReturnsEmptyPagedList()
    {
        // Arrange
        var command = new SearchProductsCommand();
        
        _repositoryMock
            .Setup(r => r.ListAsync(It.IsAny<SearchProductSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());
            
        _repositoryMock
            .Setup(r => r.CountAsync(It.IsAny<SearchProductSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var command = new SearchProductsCommand();
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(command, cancellationToken));
            
        _repositoryMock.Verify(
            r => r.ListAsync(It.IsAny<SearchProductSpecs>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var command = new SearchProductsCommand
        {
            PageNumber = 2,
            PageSize = 5
        };

        var allProducts = new List<Product>();
        for (int i = 0; i < 12; i++)
        {
            allProducts.Add(new Product($"Product {i}", $"Description {i}", 50m + i, _brandId));
        }
        
        var expectedProducts = allProducts.Skip(5).Take(5); // Page 2 (zero-based index 1)


        _repositoryMock
            .Setup(r => r.ListAsync(It.IsAny<SearchProductSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts.ToList());
            
        _repositoryMock
            .Setup(r => r.CountAsync(It.IsAny<SearchProductSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(allProducts.Count);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(allProducts.Count);
        result.TotalPages.Should().Be(3); // 12 items / 5 per page = 3 pages
    }

    [Fact]
    public async Task Handle_WithRepositoryException_ExceptionIsPropagated()
    {
        // Arrange
        var command = new SearchProductsCommand();
        var expectedException = new InvalidOperationException("Database error");
        
        _repositoryMock
            .Setup(r => r.ListAsync(It.IsAny<SearchProductSpecs>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));
            
        exception.Should().BeSameAs(expectedException);
    }

    [Fact]
    public async Task Handle_WithCountException_ExceptionIsPropagated()
    {
        // Arrange
        var command = new SearchProductsCommand();
        var expectedException = new InvalidOperationException("Count error");
        
        _repositoryMock
            .Setup(r => r.ListAsync(It.IsAny<SearchProductSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { new("Test", "Desc", 50m, _brandId) });
            
        _repositoryMock
            .Setup(r => r.CountAsync(It.IsAny<SearchProductSpecs>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));
            
        exception.Should().BeSameAs(expectedException);
    }

    [Fact]
    public async Task Handle_WithDefaultPagination_UsesDefaultValues()
    {
        // Arrange
        var command = new SearchProductsCommand();
        
        _repositoryMock
            .Setup(r => r.ListAsync(It.IsAny<SearchProductSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());
            
        _repositoryMock
            .Setup(r => r.CountAsync(It.IsAny<SearchProductSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Should use default pagination values (PageNumber=1, PageSize=10)
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
}
