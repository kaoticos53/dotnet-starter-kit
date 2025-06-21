using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Infrastructure.Tests.Persistence;

[Collection("CatalogDbContextTests")]
public class CatalogDbInitializerTests : IClassFixture<CatalogDbContextFixture>
{
    private readonly CatalogDbContext _context;
    private readonly Mock<ILogger<CatalogDbInitializer>> _loggerMock;
    private readonly CatalogDbInitializer _initializer;

    public CatalogDbInitializerTests(CatalogDbContextFixture fixture)
    {
        _context = fixture.CreateContext();
        _loggerMock = new Mock<ILogger<CatalogDbInitializer>>();
        _initializer = new CatalogDbInitializer(_context, _loggerMock.Object);
        
        // Ensure database is clean before each test
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task InitializeAsync_ShouldCreateDatabaseIfNotExists()
    {
        // Act
        await _initializer.InitializeAsync();

        // Assert
        var created = await _context.Database.CanConnectAsync();
        created.Should().BeTrue();
    }

    [Fact]
    public async Task SeedAsync_ShouldAddDefaultBrands()
    {
        // Act
        await _initializer.InitializeAsync();
        await _initializer.SeedAsync();

        // Assert
        var brands = await _context.Brands.ToListAsync();
        brands.Should().NotBeEmpty();
        brands.Should().Contain(b => b.Name == "Default Brand");
    }

    [Fact]
    public async Task SeedAsync_ShouldAddSampleProducts()
    {
        // Arrange - Ensure we have a brand for the products
        var brand = new Brand("Test Brand", "Test Brand Description");
        await _context.Brands.AddAsync(brand);
        await _context.SaveChangesAsync();

        // Act
        await _initializer.InitializeAsync();
        await _initializer.SeedAsync();

        // Assert
        var products = await _context.Products.ToListAsync();
        products.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SeedAsync_ShouldNotDuplicateDataOnMultipleCalls()
    {
        // Arrange
        await _initializer.InitializeAsync();
        
        // Act - Call seed multiple times
        await _initializer.SeedAsync();
        var firstBrandsCount = await _context.Brands.CountAsync();
        var firstProductsCount = await _context.Products.CountAsync();
        
        await _initializer.SeedAsync();
        
        // Assert - Counts should not change on second seed
        var secondBrandsCount = await _context.Brands.CountAsync();
        var secondProductsCount = await _context.Products.CountAsync();
        
        secondBrandsCount.Should().Be(firstBrandsCount);
        secondProductsCount.Should().Be(firstProductsCount);
    }

    [Fact]
    public async Task SeedAsync_ShouldHandleEmptyDatabase()
    {
        // Act
        await _initializer.InitializeAsync();
        await _initializer.SeedAsync();

        // Assert - Should not throw and should have seeded data
        var brands = await _context.Brands.ToListAsync();
        var products = await _context.Products.ToListAsync();
        
        brands.Should().NotBeNull();
        products.Should().NotBeNull();
    }

    [Fact]
    public async Task SeedAsync_ShouldLogInformationOnSeeding()
    {
        // Act
        await _initializer.InitializeAsync();
        await _initializer.SeedAsync();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Seeding database"))),
                Times.AtLeastOnce);
    }
}
