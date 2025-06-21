using Bogus;
using FSH.Starter.WebApi.Catalog.Domain;

namespace Catalog.Domain.UnitTests.Tests.Common;

/// <summary>  
/// Constructor de datos de prueba para entidades del dominio.  
/// </summary>  
public static class TestDataBuilder
{
    private static readonly Faker Faker = new();

    /// <summary>  
    /// Crea un constructor de productos con valores por defecto.  
    /// </summary>  
    public static Product BuildProduct()
    {
        return Product.Create(
            name: Faker.Commerce.ProductName(),
            description: Faker.Lorem.Sentence(),
            price: decimal.Parse(Faker.Commerce.Price(10, 1000)),
            brandId: Guid.NewGuid()
        );
    }

    /// <summary>  
    /// Genera un precio aleatorio.  
    /// </summary>  
    public static decimal GenerateRandomPrice()
    {
        return decimal.Parse(Faker.Commerce.Price(1, 1000));
    }
}
