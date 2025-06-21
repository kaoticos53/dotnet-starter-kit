using System;
using System.Threading;
using System.Threading.Tasks;
using FSH.Framework.Core.Persistence;
using FSH.Starter.WebApi.Catalog.Application.Brands.Delete.v1;
using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Domain.Exceptions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Brands.Delete.v1;

public class DeleteBrandHandlerTests
{
    private readonly Mock<ILogger<DeleteBrandHandler>> _loggerMock;
    private readonly Mock<IRepository<Brand>> _repositoryMock;
    private readonly DeleteBrandHandler _handler;
    private readonly Guid _brandId = Guid.NewGuid();
    private readonly string _brandName = "Test Brand";
    private readonly string _brandDescription = "Test Description";

    public DeleteBrandHandlerTests()
    {
        _loggerMock = new Mock<ILogger<DeleteBrandHandler>>();
        _repositoryMock = new Mock<IRepository<Brand>>();
        _handler = new DeleteBrandHandler(_loggerMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_DeletesBrandAndLogsMessage()
    {
        // Arrange
        var brand = Brand.Create(_brandName, _brandDescription);
        var command = new DeleteBrandCommand(_brandId);
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_brandId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(brand);
            
        _repositoryMock
            .Setup(r => r.DeleteAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.GetByIdAsync(_brandId, It.IsAny<CancellationToken>()),
            Times.Once);
            
        _repositoryMock.Verify(
            r => r.DeleteAsync(It.Is<Brand>(b => b.Id == _brandId), It.IsAny<CancellationToken>()),
            Times.Once);
            
        VerifyLogMessage($"Brand with id : {_brandId} deleted");
    }

    [Fact]
    public async Task Handle_WithNullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        DeleteBrandCommand? command = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _handler.Handle(command!, CancellationToken.None));
            
        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
            
        _repositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentBrand_ThrowsBrandNotFoundException()
    {
        // Arrange
        var command = new DeleteBrandCommand(_brandId);
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_brandId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Brand?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BrandNotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
            
        _repositoryMock.Verify(
            r => r.GetByIdAsync(_brandId, It.IsAny<CancellationToken>()),
            Times.Once);
            
        _repositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var command = new DeleteBrandCommand(_brandId);
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(command, cancellationToken));
            
        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
            
        _repositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ThrowsNoException()
    {
        // Arrange
        var command = new DeleteBrandCommand(Guid.Empty);
        var brand = Brand.Create(_brandName, _brandDescription);
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(brand);
            
        _repositoryMock
            .Setup(r => r.DeleteAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - No exception should be thrown
        _repositoryMock.Verify(
            r => r.GetByIdAsync(Guid.Empty, It.IsAny<CancellationToken>()),
            Times.Once);
            
        _repositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDeleteThrowsException_ExceptionIsPropagated()
    {
        // Arrange
        var brand = Brand.Create(_brandName, _brandDescription);
        var command = new DeleteBrandCommand(_brandId);
        var expectedException = new InvalidOperationException("Database error");
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_brandId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(brand);
            
        _repositoryMock
            .Setup(r => r.DeleteAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));
            
        exception.Should().BeSameAs(expectedException);
        
        _repositoryMock.Verify(
            r => r.GetByIdAsync(_brandId, It.IsAny<CancellationToken>()),
            Times.Once);
            
        _repositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()),
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
