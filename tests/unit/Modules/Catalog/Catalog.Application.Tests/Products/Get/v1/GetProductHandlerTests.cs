using System;
using System.Threading;
using System.Threading.Tasks;
using FSH.Framework.Core.Caching;
using FSH.Framework.Core.Persistence;
using FSH.Starter.WebApi.Catalog.Application.Products.Get.v1;
using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Products.Get.v1;

public class GetProductHandlerTests
{
    private readonly Mock<IReadRepository<Product>> _repositoryMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly GetProductHandler _handler;
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Product _testProduct;
    private readonly ProductResponse _testProductResponse;

    public GetProductHandlerTests()
    {
        _repositoryMock = new Mock<IReadRepository<Product>>();
        _cacheMock = new Mock<ICacheService>();
        
        var serviceProvider = new ServiceCollection()
            .AddKeyedScoped("catalog:products", sp => _repositoryMock.Object)
            .BuildServiceProvider();
            
        _handler = new GetProductHandler(
            serviceProvider.GetRequiredKeyedService<IReadRepository<Product>>("catalog:products"),
            _cacheMock.Object);

        _testProduct = new Product("Test Product", "Test Description", 9.99m, null)
        {
            Id = _productId,
            Brand = new Brand("Test Brand", "Brand Description")
        };

        _testProductResponse = new ProductResponse(
            _testProduct.Id,
            _testProduct.Name,
            _testProduct.Description,
            _testProduct.Price,
            new BrandResponse(_testProduct.Brand.Id, _testProduct.Brand.Name, _testProduct.Brand.Description));
    }

    [Fact]
    public async Task Handle_WithValidRequest_ReturnsProductFromCache()
    {
        // Arrange
        var request = new GetProductRequest(_productId);
        
        _cacheMock
            .Setup(c => c.GetOrSetAsync(
                $"product:{_productId}",
                It.IsAny<Func<Task<Product>>>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProduct);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(_testProductResponse);
        _repositoryMock.Verify(
            r => r.FirstOrDefaultAsync(It.IsAny<GetProductSpecs>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithCacheMiss_ReturnsProductFromRepository()
    {
        // Arrange
        var request = new GetProductRequest(_productId);
        
        _cacheMock
            .Setup(c => c.GetOrSetAsync(
                $"product:{_productId}",
                It.IsAny<Func<Task<Product>>>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, Func<Task<Product>> factory, TimeSpan? _, TimeSpan? __, CancellationToken _) => factory());
            
        _repositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<GetProductSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProduct);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(_testProductResponse);
        _repositoryMock.Verify(
            r => r.FirstOrDefaultAsync(It.IsAny<GetProductSpecs>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ThrowsProductNotFoundException()
    {
        // Arrange
        var request = new GetProductRequest(_productId);
        
        _cacheMock
            .Setup(c => c.GetOrSetAsync(
                $"product:{_productId}",
                It.IsAny<Func<Task<Product>>>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, Func<Task<Product>> factory, TimeSpan? _, TimeSpan? __, CancellationToken _) => factory());
            
        _repositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<GetProductSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProductNotFoundException>(() => 
            _handler.Handle(request, CancellationToken.None));
            
        exception.ProductId.Should().Be(_productId);
    }

    [Fact]
    public async Task Handle_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        GetProductRequest? request = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _handler.Handle(request!, CancellationToken.None));
            
        _cacheMock.Verify(
            c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<Product>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var request = new GetProductRequest(_productId);
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(request, cancellationToken));
            
        _cacheMock.Verify(
            c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<Product>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ThrowsProductNotFoundException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var request = new GetProductRequest(emptyGuid);
        
        _cacheMock
            .Setup(c => c.GetOrSetAsync(
                $"product:{emptyGuid}",
                It.IsAny<Func<Task<Product>>>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, Func<Task<Product>> factory, TimeSpan? _, TimeSpan? __, CancellationToken _) => factory());
            
        _repositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<GetProductSpecs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProductNotFoundException>(() => 
            _handler.Handle(request, CancellationToken.None));
            
        exception.ProductId.Should().Be(emptyGuid);
    }

    [Fact]
    public async Task Handle_WithRepositoryException_ExceptionIsPropagated()
    {
        // Arrange
        var request = new GetProductRequest(_productId);
        var expectedException = new InvalidOperationException("Database error");
        
        _cacheMock
            .Setup(c => c.GetOrSetAsync(
                $"product:{_productId}",
                It.IsAny<Func<Task<Product>>>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(request, CancellationToken.None));
            
        exception.Should().BeSameAs(expectedException);
    }
}
