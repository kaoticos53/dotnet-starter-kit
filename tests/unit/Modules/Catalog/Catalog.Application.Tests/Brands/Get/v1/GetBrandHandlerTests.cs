using System;
using System.Threading;
using System.Threading.Tasks;
using FSH.Framework.Core.Caching;
using FSH.Framework.Core.Persistence;
using FSH.Starter.WebApi.Catalog.Application.Brands.Get.v1;
using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Brands.Get.v1;

public class GetBrandHandlerTests
{
    private readonly Mock<IReadRepository<Brand>> _repositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetBrandHandler _handler;
    private readonly Guid _brandId = Guid.NewGuid();
    private readonly string _brandName = "Test Brand";
    private readonly string _brandDescription = "Test Description";

    public GetBrandHandlerTests()
    {
        _repositoryMock = new Mock<IReadRepository<Brand>>();
        _cacheServiceMock = new Mock<ICacheService>();
        
        var serviceProvider = new ServiceCollection()
            .AddKeyedScoped("catalog:brands", sp => _repositoryMock.Object)
            .BuildServiceProvider();
            
        _handler = new GetBrandHandler(
            serviceProvider.GetRequiredKeyedService<IReadRepository<Brand>>("catalog:brands"),
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ReturnsBrandFromCache()
    {
        // Arrange
        var request = new GetBrandRequest(_brandId);
        var cachedResponse = new BrandResponse(_brandId, _brandName, _brandDescription);
        
        _cacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                $"brand:{_brandId}", 
                It.IsAny<Func<Task<BrandResponse>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_brandId);
        result.Name.Should().Be(_brandName);
        result.Description.Should().Be(_brandDescription);
        
        _cacheServiceMock.Verify(
            c => c.GetOrSetAsync(
                $"brand:{_brandId}",
                It.IsAny<Func<Task<BrandResponse>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
            
        // Verify repository was not called since we're using cached response
        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithCacheMiss_ReturnsBrandFromRepository()
    {
        // Arrange
        var request = new GetBrandRequest(_brandId);
        var brand = Brand.Create(_brandName, _brandDescription);
        var expectedResponse = new BrandResponse(_brandId, _brandName, _brandDescription);
        
        _cacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                $"brand:{_brandId}", 
                It.IsAny<Func<Task<BrandResponse>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, Func<Task<BrandResponse>> factory, CancellationToken _) => factory());
            
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_brandId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(brand);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_brandId);
        result.Name.Should().Be(_brandName);
        result.Description.Should().Be(_brandDescription);
        
        _repositoryMock.Verify(
            r => r.GetByIdAsync(_brandId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentBrand_ThrowsBrandNotFoundException()
    {
        // Arrange
        var request = new GetBrandRequest(_brandId);
        
        _cacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                $"brand:{_brandId}", 
                It.IsAny<Func<Task<BrandResponse>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, Func<Task<BrandResponse>> factory, CancellationToken _) => factory());
            
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_brandId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Brand?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BrandNotFoundException>(() => 
            _handler.Handle(request, CancellationToken.None));
            
        _repositoryMock.Verify(
            r => r.GetByIdAsync(_brandId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        GetBrandRequest? request = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _handler.Handle(request!, CancellationToken.None));
            
        _cacheServiceMock.Verify(
            c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<BrandResponse>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
            
        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var request = new GetBrandRequest(_brandId);
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(request, cancellationToken));
            
        _cacheServiceMock.Verify(
            c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<BrandResponse>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
            
        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ThrowsNoException()
    {
        // Arrange
        var emptyId = Guid.Empty;
        var request = new GetBrandRequest(emptyId);
        var brand = Brand.Create(_brandName, _brandDescription);
        
        _cacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                $"brand:{emptyId}", 
                It.IsAny<Func<Task<BrandResponse>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, Func<Task<BrandResponse>> factory, CancellationToken _) => factory());
            
        _repositoryMock
            .Setup(r => r.GetByIdAsync(emptyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(brand);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert - No exception should be thrown
        result.Should().NotBeNull();
        result.Id.Should().Be(brand.Id);
        
        _repositoryMock.Verify(
            r => r.GetByIdAsync(emptyId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
