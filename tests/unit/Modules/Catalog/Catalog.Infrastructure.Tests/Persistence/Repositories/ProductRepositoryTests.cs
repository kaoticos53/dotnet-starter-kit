using Ardalis.Specification;
using FSH.Framework.Core.Domain.Contracts;
using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Infrastructure.Persistence;
using FSH.Starter.WebApi.Catalog.Infrastructure.Tests.Persistence;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Infrastructure.Tests.Persistence.Repositories;

[Collection("CatalogDbContextTests")]
public class ProductRepositoryTests : RepositoryTestBase<Product, Guid>
{
    private readonly IRepository<Brand, Guid> _brandRepository;
    private Brand _testBrand;

    public ProductRepositoryTests(CatalogDbContextFixture fixture)
        : base(fixture)
    {
        _brandRepository = new CatalogRepository<Brand, Guid>(Context);
        InitializeTestDataAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeTestDataAsync()
    {
        _testBrand = new Brand("Test Brand", "Test Brand Description");
        await _brandRepository.AddAsync(_testBrand);
        await SaveChangesAsync();
    }

    [Fact]
    public async Task AddAsync_ShouldAddProductToDatabase()
    {
        // Arrange
        var product = Product.Create(
            "Test Product",
            "Test Description",
            99.99m,
            10,
            _testBrand.Id,
            _testBrand);

        // Act
        await Repository.AddAsync(product);
        await SaveChangesAsync();

        // Assert
        var result = await Repository.GetByIdAsync(product.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Product");
        result.Description.Should().Be("Test Description");
        result.Price.Should().Be(99.99m);
        result.StockQuantity.Should().Be(10);
        result.BrandId.Should().Be(_testBrand.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingProduct()
    {
        // Arrange
        var product = Product.Create(
            "Original Name",
            "Original Description",
            50.0m,
            5,
            _testBrand.Id,
            _testBrand);

        await Repository.AddAsync(product);
        await SaveChangesAsync();

        // Act
        product.Update(
            "Updated Name",
            "Updated Description",
            75.0m,
            8,
            _testBrand.Id,
            _testBrand);


        await Repository.UpdateAsync(product);
        await SaveChangesAsync();

        // Assert
        var result = await Repository.GetByIdAsync(product.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        result.Price.Should().Be(75.0m);
        result.StockQuantity.Should().Be(8);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProductFromDatabase()
    {
        // Arrange
        var product = Product.Create(
            "Product to Delete",
            "Will be deleted",
            10.0m,
            1,
            _testBrand.Id,
            _testBrand);

        await Repository.AddAsync(product);
        await SaveChangesAsync();

        // Act
        await Repository.DeleteAsync(product);
        await SaveChangesAsync();

        // Assert
        var result = await Repository.GetByIdAsync(product.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithInclude_ShouldReturnProductWithBrand()
    {
        // Arrange
        var product = Product.Create(
            "Product with Brand",
            "Should include brand",
            100.0m,
            5,
            _testBrand.Id,
            _testBrand);

        await Repository.AddAsync(product);
        await SaveChangesAsync();

        // Act
        var result = await Repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Brand.Should().NotBeNull();
        result.Brand.Name.Should().Be(_testBrand.Name);
    }

    [Fact]
    public async Task ListAsync_WithSpecification_ShouldFilterProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            Product.Create("Product 1", "Description 1", 10.0m, 5, _testBrand.Id, _testBrand),
            Product.Create("Product 2", "Description 2", 20.0m, 10, _testBrand.Id, _testBrand),
            Product.Create("Special Product", "Special Description", 50.0m, 2, _testBrand.Id, _testBrand)
        };

        foreach (var product in products)
        {
            await Repository.AddAsync(product);
        }
        await SaveChangesAsync();

        // Act
        var result = await Repository.ListAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Select(p => p.Name).Should().Contain(new[] { "Product 1", "Product 2", "Special Product" });
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var products = new List<Product>
        {
            Product.Create("Product A", "Desc A", 10.0m, 1, _testBrand.Id, _testBrand),
            Product.Create("Product B", "Desc B", 20.0m, 2, _testBrand.Id, _testBrand)
        };

        foreach (var product in products)
        {
            await Repository.AddAsync(product);
        }
        await SaveChangesAsync();

        // Act
        var count = await Repository.CountAsync();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnTrueForExistingProduct()
    {
        // Arrange
        var product = Product.Create(
            "Existing Product",
            "Should exist",
            100.0m,
            1,
            _testBrand.Id,
            _testBrand);

        await Repository.AddAsync(product);
        await SaveChangesAsync();

        // Act
        var exists = await Repository.AnyAsync(p => p.Name == "Existing Product");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WithInclude_ShouldReturnProductWithBrand()
    {
        // Arrange
        var product = Product.Create(
            "Special Product",
            "Special Description",
            150.0m,
            3,
            _testBrand.Id,
            _testBrand);

        await Repository.AddAsync(product);
        await SaveChangesAsync();

        // Act
        var result = await Repository.FirstOrDefaultAsync(p => p.Name == "Special Product");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Special Product");
        result.Brand.Should().NotBeNull();
        result.Brand.Name.Should().Be(_testBrand.Name);
    }
}
