using System;
using System.Threading;
using System.Threading.Tasks;
using FSH.Framework.Core.Persistence;
using FSH.Starter.WebApi.Catalog.Application.Brands.Create.v1;
using FSH.Starter.WebApi.Catalog.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Brands.Create.v1;

public class CreateBrandHandlerTests
{
    private readonly Mock<ILogger<CreateBrandHandler>> _loggerMock;
    private readonly Mock<IRepository<Brand>> _repositoryMock;
    private readonly CreateBrandHandler _handler;

    public CreateBrandHandlerTests()
    {
        _loggerMock = new Mock<ILogger<CreateBrandHandler>>();
        _repositoryMock = new Mock<IRepository<Brand>>();
        _handler = new CreateBrandHandler(_loggerMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsCreateBrandResponse()
    {
        // Arrange
        var command = new CreateBrandCommand("Test Brand", "Test Description");
        var cancellationToken = new CancellationToken();
        
        Brand? addedBrand = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()))
            .Callback<Brand, CancellationToken>((brand, _) => addedBrand = brand)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Brand>(), cancellationToken),
            Times.Once);

        addedBrand.Should().NotBeNull();
        addedBrand!.Name.Should().Be(command.Name);
        addedBrand.Description.Should().Be(command.Description);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"brand created {addedBrand.Id}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        CreateBrandCommand? command = null;
        var cancellationToken = new CancellationToken();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _handler.Handle(command!, cancellationToken));
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var command = new CreateBrandCommand("Test Brand", "Test Description");
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(command, cancellationToken));
    }

    [Fact]
    public async Task Handle_WithRepositoryException_LogsErrorAndRethrows()
    {
        // Arrange
        var command = new CreateBrandCommand("Test Brand", "Test Description");
        var cancellationToken = new CancellationToken();
        var exception = new InvalidOperationException("Test exception");

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, cancellationToken));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error creating brand")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
