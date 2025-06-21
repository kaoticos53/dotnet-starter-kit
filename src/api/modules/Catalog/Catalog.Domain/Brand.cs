using FSH.Framework.Core.Domain;
using FSH.Framework.Core.Domain.Contracts;
using FSH.Starter.WebApi.Catalog.Domain.Events;

namespace FSH.Starter.WebApi.Catalog.Domain;
public class Brand : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private Brand() { }

    private Brand(Guid id, string name, string? description)
    {
        Id = id;
        Name = name;
        Description = description;
        QueueDomainEvent(new BrandCreated { Brand = this });
    }

    public static Brand Create(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
        }

        return new Brand(Guid.NewGuid(), name, description);
    }

    public Brand Update(string? name, string? description)
    {
        bool isUpdated = false;

        if (name != null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be empty or whitespace.", nameof(name));
            }

            if (!string.Equals(Name, name, StringComparison.OrdinalIgnoreCase))
            {
                Name = name;
                isUpdated = true;
            }
        }

        if (description != null && !string.Equals(Description, description, StringComparison.OrdinalIgnoreCase))
        {
            Description = description;
            isUpdated = true;
        }

        if (isUpdated)
        {
            QueueDomainEvent(new BrandUpdated { Brand = this });
        }

        return this;
    }
}


