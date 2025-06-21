using Bogus;
using FSH.Starter.WebApi.Catalog.Domain;

namespace Catalog.Infrastructure.IntegrationTests.TestData;

/// <summary>
/// Constructor de datos de prueba para pruebas de integración.
/// </summary>
public static class TestDataBuilder
{
    private static readonly Faker Faker = new();
    private static int _brandCounter = 1;
    private static int _productCounter = 1;

    /// <summary>
    /// Crea un constructor de marcas con valores por defecto.
    /// </summary>
    public static Brand BuildBrand(Guid? id = null, string? name = null, string? description = null)
    {
        // Use the static Create method from the Brand class
        var brand = Brand.Create(
            name ?? $"Test Brand {_brandCounter++}",
            description ?? Faker.Lorem.Sentence());
            
        // Set the Id if provided (this is only for testing purposes)
        if (id.HasValue)
        {
            typeof(Brand).GetProperty("Id")?.SetValue(brand, id.Value);
        }
        
        return brand;
    }

    /// <summary>
    /// Crea un constructor de productos con valores por defecto.
    /// </summary>
    public static Product BuildProduct(Guid? id = null, string? name = null, string? description = null, 
        decimal? price = null, Guid? brandId = null, Brand? brand = null)
    {
        var productName = name ?? $"Test Product {_productCounter++}";
        var productPrice = price ?? Faker.Random.Decimal(10, 1000);
        
        // Use the static Create method from the Product class
        var product = Product.Create(
            productName,
            description ?? Faker.Lorem.Sentence(),
            productPrice,
            brandId);
            
        // Set the Id if provided (this is only for testing purposes)
        if (id.HasValue)
        {
            typeof(Product).GetProperty("Id")?.SetValue(product, id.Value);
        }
        
        // Set the Brand navigation property if provided
        if (brand != null)
        {
            typeof(Product).GetProperty("Brand")?.SetValue(product, brand);
        }
        
        return product;
    }

    /// <summary>
    /// Crea una lista de marcas de prueba.
    /// </summary>
    public static List<Brand> BuildBrands(int count)
    {
        var brands = new List<Brand>();
        for (int i = 0; i < count; i++)
        {
            brands.Add(BuildBrand());
        }
        return brands;
    }

    /// <summary>
    /// Crea una lista de productos de prueba.
    /// </summary>
    public static List<Product> BuildProducts(int count, Guid? brandId = null, Brand? brand = null)
    {
        var products = new List<Product>();
        for (int i = 0; i < count; i++)
        {
            products.Add(BuildProduct(brandId: brandId, brand: brand));
        }
        return products;
    }
}
