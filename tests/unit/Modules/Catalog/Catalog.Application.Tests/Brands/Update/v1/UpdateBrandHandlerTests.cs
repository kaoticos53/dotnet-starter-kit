using System;
using System.Threading;
using System.Threading.Tasks;
using FSH.Framework.Core.Persistence;
using FSH.Starter.WebApi.Catalog.Application.Brands.Update.v1;
using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Brands.Update.v1;

public class UpdateBrandHandlerTests
{
    private readonly Mock<ILogger<UpdateBrandHandler>> _loggerMock;
    private readonly Mock<IRepository<Brand>> _repositoryMock;
    private readonly UpdateBrandHandler _handler;
    private readonly Guid _brandId = Guid.NewGuid();
    private readonly string _originalName = "Original Name";
    private readonly string _originalDescription = "Original Description";
    private readonly string _updatedName = "Updated Name";
    private readonly string _updatedDescription = "Updated Description";

    public UpdateBrandHandlerTests()
    {
        _loggerMock = new Mock<ILogger<UpdateBrandHandler>>();
        _repositoryMock = new Mock<IRepository<Brand>>();
        _handler = new UpdateBrandHandler(_loggerMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesBrandAndReturnsResponse()
    {
        // Arrange
        var brand = Brand.Create(_originalName, _originalDescription);
        var command = new UpdateBrandCommand(_brandId, _updatedName, _updatedDescription);
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_brandId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(brand);
            
        Brand? updatedBrand = null;
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()))
            .Callback<Brand, CancellationToken>((b, _) => updatedBrand = b)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_brandId);
        
        updatedBrand.Should().NotBeNull();
        updatedBrand!.Name.Should().Be(_updatedName);
        updatedBrand.Description.Should().Be(_updatedDescription);
        
        _repositoryMock.Verify(
            r => r.GetByIdAsync(_brandId, It.IsAny<CancellationToken>()),
            Times.Once);
            
        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()),
            Times.Once);
            
        VerifyLogMessage($"Brand with id : {_brandId} updated.");
    }

    [Fact]
    public async Task Handle_WithNullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        UpdateBrandCommand? command = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _handler.Handle(command!, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNonExistentBrand_ThrowsBrandNotFoundException()
    {
        // Arrange
        var command = new UpdateBrandCommand(_brandId, _updatedName, _updatedDescription);
        
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
            r => r.UpdateAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullName_UpdatesOnlyDescription()
    {
        // Arrange
        var brand = Brand.Create(_originalName, _originalDescription);
        var command = new UpdateBrandCommand(_brandId, null, _updatedDescription);
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_brandId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(brand);
            
        Brand? updatedBrand = null;
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()))
            .Callback<Brand, CancellationToken>((b, _) => updatedBrand = b)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        updatedBrand.Should().NotBeNull();
        updatedBrand!.Name.Should().Be(_originalName); // Name should not change
        updatedBrand.Description.Should().Be(_updatedDescription);
    }

    [Fact]
    public async Task Handle_WithNullDescription_UpdatesOnlyName()
    {
        // Arrange
        var brand = Brand.Create(_originalName, _originalDescription);
        var command = new UpdateBrandCommand(_brandId, _updatedName, null);
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_brandId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(brand);
            
        Brand? updatedBrand = null;
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()))
            .Callback<Brand, CancellationToken>((b, _) => updatedBrand = b)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        updatedBrand.Should().NotBeNull();
        updatedBrand!.Name.Should().Be(_updatedName);
        updatedBrand.Description.Should().Be(_originalDescription); // Description should not change
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var command = new UpdateBrandCommand(_brandId, _updatedName, _updatedDescription);
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(command, cancellationToken));
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
