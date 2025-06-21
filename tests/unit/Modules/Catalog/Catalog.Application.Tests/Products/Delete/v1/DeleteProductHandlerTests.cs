using System;
using System.Threading;
using System.Threading.Tasks;
using FSH.Framework.Core.Persistence;
using FSH.Starter.WebApi.Catalog.Application.Products.Delete.v1;
using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Products.Delete.v1;

public class DeleteProductHandlerTests
{
    private readonly Mock<ILogger<DeleteProductHandler>> _loggerMock;
    private readonly Mock<IRepository<Product>> _repositoryMock;
    private readonly DeleteProductHandler _handler;
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Product _testProduct;

    public DeleteProductHandlerTests()
    {
        _loggerMock = new Mock<ILogger<DeleteProductHandler>>();
        _repositoryMock = new Mock<IRepository<Product>>();
        
        var serviceProvider = new ServiceCollection()
            .AddKeyedScoped("catalog:products", sp => _repositoryMock.Object)
            .BuildServiceProvider();
            
        _handler = new DeleteProductHandler(
            _loggerMock.Object,
            serviceProvider.GetRequiredKeyedService<IRepository<Product>>("catalog:products"));

        _testProduct = Product.Create("Test Product", "Test Description", 9.99m, Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_WithValidCommand_DeletesProductAndLogs()
    {
        // Arrange
        var command = new DeleteProductCommand(_productId);
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProduct);
            
        _repositoryMock
            .Setup(r => r.DeleteAsync(_testProduct, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()),
            Times.Once);
            
        _repositoryMock.Verify(
            r => r.DeleteAsync(_testProduct, It.IsAny<CancellationToken>()),
            Times.Once);
            
        VerifyLogMessage($"product with id : {_productId} deleted");
    }

    [Fact]
    public async Task Handle_WithNullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        DeleteProductCommand? command = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _handler.Handle(command!, CancellationToken.None));
            
        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
            
        _repositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ThrowsProductNotFoundException()
    {
        // Arrange
        var command = new DeleteProductCommand(_productId);
        
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
            r => r.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var command = new DeleteProductCommand(_productId);
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(command, cancellationToken));
            
        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
            
        _repositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDeleteThrowsException_ExceptionIsPropagated()
    {
        // Arrange
        var command = new DeleteProductCommand(_productId);
        var expectedException = new InvalidOperationException("Database error");
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProduct);
            
        _repositoryMock
            .Setup(r => r.DeleteAsync(_testProduct, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));
            
        exception.Should().BeSameAs(expectedException);
        
        _repositoryMock.Verify(
            r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()),
            Times.Once);
            
        _repositoryMock.Verify(
            r => r.DeleteAsync(_testProduct, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ThrowsProductNotFoundException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var command = new DeleteProductCommand(emptyGuid);
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(emptyGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProductNotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
            
        exception.ProductId.Should().Be(emptyGuid);
        
        _repositoryMock.Verify(
            r => r.GetByIdAsync(emptyGuid, It.IsAny<CancellationToken>()),
            Times.Once);
            
        _repositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenGetByIdThrowsException_ExceptionIsPropagated()
    {
        // Arrange
        var command = new DeleteProductCommand(_productId);
        var expectedException = new InvalidOperationException("Database error");
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));
            
        exception.Should().BeSameAs(expectedException);
        
        _repositoryMock.Verify(
            r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()),
            Times.Once);
            
        _repositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
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
