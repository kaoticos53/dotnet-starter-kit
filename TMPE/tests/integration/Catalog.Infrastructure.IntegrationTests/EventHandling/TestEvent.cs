using MediatR;

namespace Catalog.Infrastructure.IntegrationTests.EventHandling;

/// <summary>
/// Evento de prueba para verificar el manejo de eventos.
/// </summary>
public class TestEvent : INotification
{
    public bool WasHandled { get; set; }
}

/// <summary>
/// Manejador de eventos de prueba.
/// </summary>
public class TestEventHandler : INotificationHandler<TestEvent>
{
    public Task Handle(TestEvent notification, CancellationToken cancellationToken)
    {
        notification.WasHandled = true;
        return Task.CompletedTask;
    }
}
