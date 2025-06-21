using FSH.Framework.Core.Domain;
using FSH.Framework.Core.Domain.Contracts;
using FSH.Starter.WebApi.Catalog.Domain.Events;

namespace FSH.Starter.WebApi.Catalog.Domain;
public class Product : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public Guid? BrandId { get; private set; }
    public virtual Brand Brand { get; private set; } = default!;

    private Product() { }

    private Product(Guid id, string name, string? description, decimal price, Guid? brandId)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        BrandId = brandId;

        QueueDomainEvent(new ProductCreated { Product = this });
    }

    public static Product Create(string name, string? description, decimal price, Guid? brandId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
        }

        if (price <= 0)
        {
            throw new ArgumentException("Price must be greater than zero.", nameof(price));
        }

        return new Product(Guid.NewGuid(), name, description, price, brandId);
    }

    public Product Update(string? name, string? description, decimal? price, Guid? brandId)
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

        if (price.HasValue)
        {
            if (price.Value <= 0)
            {
                throw new ArgumentException("Price must be greater than zero.", nameof(price));
            }

            if (Price != price.Value)
            {
                Price = price.Value;
                isUpdated = true;
            }
        }

        if (brandId.HasValue && brandId.Value != Guid.Empty && BrandId != brandId.Value)
        {
            BrandId = brandId.Value;
            isUpdated = true;
        }

        if (isUpdated)
        {
            QueueDomainEvent(new ProductUpdated { Product = this });
        }

        return this;
    }
}

