using Catalog.Infrastructure.IntegrationTests.Fixtures;
using Catalog.Infrastructure.IntegrationTests.TestData;
using FSH.Starter.WebApi.Catalog.Application.Brands.EventHandlers;
using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Domain.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Catalog.Infrastructure.IntegrationTests.EventHandling;

/// <summary>
/// Contiene pruebas de integración para los manejadores de eventos de Brand.
/// </summary>
public class BrandEventHandlersTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly Mock<ILogger<BrandCreatedEventHandler>> _loggerMock;
    private readonly ServiceProvider _serviceProvider;

    public BrandEventHandlersTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _loggerMock = new Mock<ILogger<BrandCreatedEventHandler>>();
        
        var services = new ServiceCollection();
        
        // Registrar MediatR para que resuelva los handlers y dependencias correctamente
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<BrandCreatedEventHandler>());
        
        // Reemplazar el logger solo para BrandCreatedEventHandler
        services.AddSingleton(_loggerMock.Object);

        services.AddTransient<INotificationHandler<BrandCreated>, BrandCreatedEventHandler>(sp =>
            new BrandCreatedEventHandler(_loggerMock.Object));
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Handle_BrandCreatedEvent_ShouldLogAppropriateMessages()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var brand = TestDataBuilder.BuildBrand();
        var notification = new BrandCreated { Brand = brand };

        // Configurar el mock para verificar las llamadas de log
        _loggerMock.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("handling brand created domain event")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ));

        _loggerMock.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("finished handling brand created domain event")),
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
    public async Task Handle_BrandCreatedEvent_ShouldCompleteSuccessfully()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var brand = TestDataBuilder.BuildBrand();
        var notification = new BrandCreated { Brand = brand };

        // Act & Assert (no debería lanzar excepciones)
        var exception = await Record.ExceptionAsync(() => mediator.Publish(notification));
        
        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NullBrand_ShouldNotThrow()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var notification = new BrandCreated { Brand = null! };

        // Act & Assert (no debería lanzar excepciones)
        var exception = await Record.ExceptionAsync(() => mediator.Publish(notification));
        
        // Assert
        exception.Should().BeNull();
    }
}
