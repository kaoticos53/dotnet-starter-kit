using FSH.Starter.WebApi.Catalog.Application.Brands.EventHandlers;
using FSH.Starter.WebApi.Catalog.Domain.Events;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Brands.EventHandlers;

public class BrandCreatedEventHandlerTests
{
    private readonly Mock<ILogger<BrandCreatedEventHandler>> _loggerMock;
    private readonly BrandCreatedEventHandler _handler;

    public BrandCreatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<BrandCreatedEventHandler>>();
        _handler = new BrandCreatedEventHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCalled_LogsInformation()
    {
        // Arrange
        var brand = new Domain.Brand(Guid.NewGuid(), "Test Brand", "Test Description");
        var notification = new BrandCreated { Brand = brand };
        var cancellationToken = new CancellationToken();

        // Setup logger verification
        _loggerMock.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("handling brand created domain event")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        await _handler.Handle(notification, cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("handling brand created domain event")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("finished handling brand created domain event")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullBrand_LogsInformation()
    {
        // Arrange
        var notification = new BrandCreated { Brand = null! };
        var cancellationToken = new CancellationToken();

        // Act
        await _handler.Handle(notification, cancellationToken);

        // Assert - Just verify no exception is thrown
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("handling brand created domain event")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var brand = new Domain.Brand(Guid.NewGuid(), "Test Brand", "Test Description");
        var notification = new BrandCreated { Brand = brand };
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(notification, cancellationToken));
    }
}
