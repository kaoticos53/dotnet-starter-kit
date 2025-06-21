using Catalog.Infrastructure.IntegrationTests.Fixtures;
using Catalog.Infrastructure.IntegrationTests.TestData;
using FSH.Starter.WebApi.Catalog.Application.Products.EventHandlers;
using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Domain.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Catalog.Infrastructure.IntegrationTests.EventHandling;

/// <summary>
/// Contiene pruebas de integración para los manejadores de eventos de Product.
/// </summary>
public class ProductEventHandlersTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly Mock<ILogger<ProductCreatedEventHandler>> _loggerMock;
    private readonly ServiceProvider _serviceProvider;

    public ProductEventHandlersTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _loggerMock = new Mock<ILogger<ProductCreatedEventHandler>>();
        
        var services = new ServiceCollection();
        
        // Registrar el manejador de eventos real
        services.AddTransient<INotificationHandler<ProductCreated>, ProductCreatedEventHandler>();
        
        // Registrar dependencias mockeadas
        services.AddSingleton(_loggerMock.Object);
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Handle_ProductCreatedEvent_ShouldLogAppropriateMessages()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var product = TestDataBuilder.BuildProduct();
        var notification = new ProductCreated { Product = product };

        // Configurar el mock para verificar las llamadas de log
        _loggerMock.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("handling product created domain event")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ));

        _loggerMock.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("finished handling product created domain event")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ));

        // Act
        await mediator.Publish(notification);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("handling product created domain event")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("finished handling product created domain event")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ProductCreatedEvent_ShouldCompleteSuccessfully()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var product = TestDataBuilder.BuildProduct();
        var notification = new ProductCreated { Product = product };

        // Act & Assert (no debería lanzar excepciones)
        var exception = await Record.ExceptionAsync(() => mediator.Publish(notification));
        
        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NullProduct_ShouldNotThrow()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var notification = new ProductCreated { Product = null! };

        // Act & Assert (no debería lanzar excepciones)
        var exception = await Record.ExceptionAsync(() => mediator.Publish(notification));
        
        // Assert
        exception.Should().BeNull();
    }
}
