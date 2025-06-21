using Catalog.Infrastructure.IntegrationTests.Fixtures;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Catalog.Infrastructure.IntegrationTests.EventHandling;

/// <summary>
/// Clase base para pruebas de manejadores de eventos.
/// </summary>
public abstract class BaseEventHandlerTests : IClassFixture<TestDatabaseFixture>
{
    protected readonly TestDatabaseFixture Fixture;
    protected readonly ServiceProvider ServiceProvider;
    protected readonly IPublisher Publisher;

    protected BaseEventHandlerTests(TestDatabaseFixture fixture)
    {
        Fixture = fixture;
        
        var services = new ServiceCollection();
        
        // Registrar servicios necesarios para el manejo de eventos
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssemblyContaining<TestEventHandler>());
            
        ServiceProvider = services.BuildServiceProvider();
        Publisher = ServiceProvider.GetRequiredService<IPublisher>();
    }
}
