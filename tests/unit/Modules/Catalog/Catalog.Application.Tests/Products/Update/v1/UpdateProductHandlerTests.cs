using System;
using System.Threading;
using System.Threading.Tasks;
using FSH.Framework.Core.Persistence;
using FSH.Starter.WebApi.Catalog.Application.Products.Update.v1;
using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Products.Update.v1;

public class UpdateProductHandlerTests
{
    private readonly Mock<ILogger<UpdateProductHandler>> _loggerMock;
    private readonly Mock<IRepository<Product>> _repositoryMock;
    private readonly UpdateProductHandler _handler;
    private readonly Guid _productId = Guid.NewGuid();
    private const string OriginalName = "Original Product";
    private const string OriginalDescription = "Original Description";
    private const decimal OriginalPrice = 9.99m;
    private readonly Guid? OriginalBrandId = Guid.NewGuid();
    private const string UpdatedName = "Updated Product";
    private const string UpdatedDescription = "Updated Description";
    private const decimal UpdatedPrice = 19.99m;
    private readonly Guid? UpdatedBrandId = Guid.NewGuid();

    public UpdateProductHandlerTests()
    {
        _loggerMock = new Mock<ILogger<UpdateProductHandler>>();
        _repositoryMock = new Mock<IRepository<Product>>();
        
        var serviceProvider = new ServiceCollection()
            .AddKeyedScoped("catalog:products", sp => _repositoryMock.Object)
            .BuildServiceProvider();
            
        _handler = new UpdateProductHandler(
            _loggerMock.Object,
            serviceProvider.GetRequiredKeyedService<IRepository<Product>>("catalog:products"));
    }

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesProductAndReturnsResponse()
    {
        // Arrange
        var originalProduct = Product.Create(OriginalName, OriginalDescription, OriginalPrice, OriginalBrandId);
        var command = new UpdateProductCommand(
            _productId,
            UpdatedName,
            UpdatedPrice,
            UpdatedDescription,
            UpdatedBrandId);
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalProduct);
            
        Product? updatedProduct = null;
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => updatedProduct = p)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_productId);
        
        updatedProduct.Should().NotBeNull();
        updatedProduct!.Name.Should().Be(UpdatedName);
        updatedProduct.Description.Should().Be(UpdatedDescription);
        updatedProduct.Price.Should().Be(UpdatedPrice);
        updatedProduct.BrandId.Should().Be(UpdatedBrandId);
        
        _repositoryMock.Verify(
            r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()),
            Times.Once);
            
        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Once);
            
        VerifyLogMessage($"product with id : {_productId} updated.");
    }

    [Fact]
    public async Task Handle_WithNullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        UpdateProductCommand? command = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _handler.Handle(command!, CancellationToken.None));
            
        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
            
        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ThrowsProductNotFoundException()
    {
        // Arrange
        var command = new UpdateProductCommand(_productId, UpdatedName, UpdatedPrice);
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProductNotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
            
        exception.ProductId.Should().Be(_productId);
        
        _repositoryMock.Verify(
            r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()),
            Times.Once);
            
        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullName_UpdatesProductWithNullName()
    {
        // Arrange
        var originalProduct = Product.Create(OriginalName, OriginalDescription, OriginalPrice, OriginalBrandId);
        var command = new UpdateProductCommand(_productId, null, UpdatedPrice);
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalProduct);
            
        Product? updatedProduct = null;
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => updatedProduct = p)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        updatedProduct.Should().NotBeNull();
        updatedProduct!.Name.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNullBrandId_UpdatesProductWithNullBrandId()
    {
        // Arrange
        var originalProduct = Product.Create(OriginalName, OriginalDescription, OriginalPrice, OriginalBrandId);
        var command = new UpdateProductCommand(_productId, UpdatedName, UpdatedPrice, null, null);
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalProduct);
            
        Product? updatedProduct = null;
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => updatedProduct = p)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        updatedProduct.Should().NotBeNull();
        updatedProduct!.BrandId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var command = new UpdateProductCommand(_productId, UpdatedName, UpdatedPrice);
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(command, cancellationToken));
            
        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
            
        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUpdateThrowsException_ExceptionIsPropagated()
    {
        // Arrange
        var originalProduct = Product.Create(OriginalName, OriginalDescription, OriginalPrice, OriginalBrandId);
        var command = new UpdateProductCommand(_productId, UpdatedName, UpdatedPrice);
        var expectedException = new InvalidOperationException("Database error");
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalProduct);
            
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));
            
        exception.Should().BeSameAs(expectedException);
        
        _repositoryMock.Verify(
            r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()),
            Times.Once);
            
        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
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
