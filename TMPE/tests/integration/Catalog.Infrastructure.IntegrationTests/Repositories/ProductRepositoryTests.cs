using Ardalis.Specification;
using Catalog.Infrastructure.IntegrationTests.Fixtures;
using Catalog.Infrastructure.IntegrationTests.TestData;
using FSH.Framework.Core.Domain.Contracts;
using FSH.Framework.Core.Persistence;
using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Catalog.Infrastructure.IntegrationTests.Repositories;

/// <summary>
/// Contiene pruebas de integración para el repositorio de Product.
/// </summary>
public class ProductRepositoryTests : BaseIntegrationTest
{
    private readonly CatalogRepository<Product> _repository;
    private readonly ITestOutputHelper _output;
    private readonly CatalogRepository<Brand> _brandRepository;

    public ProductRepositoryTests(TestDatabaseFixture fixture, ITestOutputHelper output) 
        : base(fixture)
    {
        _output = output;
        _repository = new CatalogRepository<Product>(Context);
        _brandRepository = new CatalogRepository<Brand>(Context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddProductToDatabase()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var product = TestDataBuilder.BuildProduct(brandId: brand.Id, brand: brand);

        // Act
        await _repository.AddAsync(product);
        await Context.SaveChangesAsync();

        // Assert
        var savedProduct = await Context.Products
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.Id == product.Id);
            
        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be(product.Name);
        savedProduct.Description.Should().Be(product.Description);
        savedProduct.Price.Should().Be(product.Price);
        savedProduct.BrandId.Should().Be(brand.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProductWithBrand_WhenProductExists()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var product = TestDataBuilder.BuildProduct(brandId: brand.Id, brand: brand);
        await Context.Products.AddAsync(product);
        await Context.SaveChangesAsync();

        // Act
        var spec = new ProductByIdWithBrandSpec(product.Id);
        var result = await _repository.FirstOrDefaultAsync(spec);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be(product.Name);
        result.Brand.Should().NotBeNull();
        result.Brand!.Id.Should().Be(brand.Id);
    }

    [Fact]
    public async Task ListAsync_ShouldFilterProductsByBrand()
    {
        // Arrange
        var brand1 = await CreateBrandAsync();
        var brand2 = await CreateBrandAsync();
        
        var brand1Products = TestDataBuilder.BuildProducts(3, brandId: brand1.Id, brand: brand1);
        var brand2Products = TestDataBuilder.BuildProducts(2, brandId: brand2.Id, brand: brand2);
        
        await Context.Products.AddRangeAsync(brand1Products);
        await Context.Products.AddRangeAsync(brand2Products);
        await Context.SaveChangesAsync();

        // Act
        var spec = new ProductsByBrandSpec(brand1.Id);
        var results = await _repository.ListAsync(spec);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(3);
        results.Should().OnlyContain(p => p.BrandId == brand1.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProductInDatabase()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var product = TestDataBuilder.BuildProduct(brandId: brand.Id, brand: brand);
        await Context.Products.AddAsync(product);
        await Context.SaveChangesAsync();

        // Act
        // Get a fresh instance of the product from the database
        var productToUpdate = await Context.Products.FindAsync(product.Id);
        productToUpdate.Should().NotBeNull();
        
        // Update the product using its Update method
        productToUpdate!.Update(
            "Updated " + product.Name,
            "Updated " + product.Description,
            product.Price + 100,
            brand.Id);
            
        // Save changes
        await _repository.UpdateAsync(productToUpdate);
        await Context.SaveChangesAsync();

        // Assert
        var result = await Context.Products.FindAsync(product.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be(productToUpdate.Name);
        result.Description.Should().Be(productToUpdate.Description);
        result.Price.Should().Be(productToUpdate.Price);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProductFromDatabase()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var product = TestDataBuilder.BuildProduct(brandId: brand.Id, brand: brand);
        await Context.Products.AddAsync(product);
        await Context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(product);
        await Context.SaveChangesAsync();

        // Assert
        var result = await Context.Products.FindAsync(product.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var products = TestDataBuilder.BuildProducts(5, brandId: brand.Id, brand: brand);
        await Context.Products.AddRangeAsync(products);
        await Context.SaveChangesAsync();

        // Act
        var count = await _repository.CountAsync();

        // Assert
        count.Should().Be(5);
    }

    private async Task<Brand> CreateBrandAsync()
    {
        var brand = TestDataBuilder.BuildBrand();
        await _brandRepository.AddAsync(brand);
        await Context.SaveChangesAsync();
        return brand;
    }
}

/// <summary>
/// Especificación para obtener un producto por su ID incluyendo la marca relacionada.
/// </summary>
public class ProductByIdWithBrandSpec : Specification<Product>, ISingleResultSpecification
{
    public ProductByIdWithBrandSpec(Guid productId)
    {
        Query
            .Where(p => p.Id == productId)
            .Include(p => p.Brand);
    }
}

/// <summary>
/// Especificación para filtrar productos por marca.
/// </summary>
public class ProductsByBrandSpec : Specification<Product>
{
    public ProductsByBrandSpec(Guid brandId)
    {
        Query
            .Where(p => p.BrandId == brandId)
            .Include(p => p.Brand);
    }
}
