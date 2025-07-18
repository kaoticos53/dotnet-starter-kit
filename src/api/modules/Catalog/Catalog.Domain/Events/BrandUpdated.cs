using FSH.Framework.Core.Domain.Events;

namespace FSH.Starter.WebApi.Catalog.Domain.Events;

public sealed record BrandUpdated : DomainEvent
{
    public Brand? Brand { get; set; }
    
    public static string EventType => nameof(BrandUpdated);
}
