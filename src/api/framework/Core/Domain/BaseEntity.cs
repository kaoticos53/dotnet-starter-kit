using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using FSH.Framework.Core.Domain.Contracts;
using FSH.Framework.Core.Domain.Events;

namespace FSH.Framework.Core.Domain;

public abstract class BaseEntity<TId> : IEntity<TId>
{
    public TId Id { get; protected init; } = default!;
    [NotMapped]
    public Collection<DomainEvent> DomainEvents { get; } = new Collection<DomainEvent>();
    public void QueueDomainEvent(DomainEvent @event)
    {
        if (!DomainEvents.Contains(@event))
            DomainEvents.Add(@event);
    }

    /// <summary>
    /// Limpia todos los eventos de dominio pendientes en la entidad.
    /// </summary>
    public void ClearDomainEvents()
    {
        DomainEvents.Clear();
    }
}

public abstract class BaseEntity : BaseEntity<Guid>
{
    protected BaseEntity() => Id = Guid.NewGuid();
}
