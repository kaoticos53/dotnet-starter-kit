using System;
using System.Threading;
using System.Threading.Tasks;
using FSH.Framework.Core.Persistence;
using FSH.Starter.WebApi.Catalog.Application.Products.Create.v1;
using FSH.Starter.WebApi.Catalog.Domain;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Products.Create.v1;

public class CreateProductHandlerTests
{
    private readonly Mock<ILogger<CreateProductHandler>> _loggerMock;
    private readonly Mock<IRepository<Product>> _repositoryMock;
    private readonly CreateProductHandler _handler;
    private readonly string _productName = "Test Product";
    private readonly string _productDescription = "Test Description";
    private readonly decimal _productPrice = 9.99m;
    private readonly Guid? _brandId = Guid.NewGuid();

    public CreateProductHandlerTests()
    {
        _loggerMock = new Mock<ILogger<CreateProductHandler>>();
        _repositoryMock = new Mock<IRepository<Product>>();
        
        var serviceProvider = new ServiceCollection()
            .AddKeyedScoped("catalog:products", sp => _repositoryMock.Object)
            .BuildServiceProvider();
            
        _handler = new CreateProductHandler(
            _loggerMock.Object,
            serviceProvider.GetRequiredKeyedService<IRepository<Product>>("catalog:products"));
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesProductAndReturnsResponse()
    {
        // Arrange
        var command = new CreateProductCommand(_productName, _productPrice, _productDescription, _brandId);
        
        Product? createdProduct = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => createdProduct = p)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNull();
        
        createdProduct.Should().NotBeNull();
        createdProduct!.Name.Should().Be(_productName);
        createdProduct.Description.Should().Be(_productDescription);
        createdProduct.Price.Should().Be(_productPrice);
        createdProduct.BrandId.Should().Be(_brandId);
        
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Once);
            
        VerifyLogMessage($"product created {createdProduct.Id}");
    }

    [Fact]
    public async Task Handle_WithNullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        CreateProductCommand? command = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _handler.Handle(command!, CancellationToken.None));
            
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullName_ThrowsNoException()
    {
        // Arrange
        var command = new CreateProductCommand(null, _productPrice, _productDescription, _brandId);
        
        Product? createdProduct = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => createdProduct = p)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - No exception should be thrown, validation is handled by the validator
        result.Should().NotBeNull();
        createdProduct.Should().NotBeNull();
        createdProduct!.Name.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNullBrandId_CreatesProductWithNullBrandId()
    {
        // Arrange
        var command = new CreateProductCommand(_productName, _productPrice, _productDescription, null);
        
        Product? createdProduct = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => createdProduct = p)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        createdProduct.Should().NotBeNull();
        createdProduct!.BrandId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithEmptyBrandId_CreatesProductWithEmptyBrandId()
    {
        // Arrange
        var command = new CreateProductCommand(_productName, _productPrice, _productDescription, Guid.Empty);
        
        Product? createdProduct = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => createdProduct = p)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        createdProduct.Should().NotBeNull();
        createdProduct!.BrandId.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var command = new CreateProductCommand(_productName, _productPrice, _productDescription, _brandId);
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(command, cancellationToken));
            
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ExceptionIsPropagated()
    {
        // Arrange
        var command = new CreateProductCommand(_productName, _productPrice, _productDescription, _brandId);
        var expectedException = new InvalidOperationException("Database error");
        
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));
            
        exception.Should().BeSameAs(expectedException);
        
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void VerifyLogMessage(string expectedMessage)
    {
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
