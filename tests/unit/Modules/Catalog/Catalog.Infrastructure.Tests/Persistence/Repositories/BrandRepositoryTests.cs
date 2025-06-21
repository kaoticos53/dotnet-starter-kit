using Ardalis.Specification;
using FSH.Framework.Core.Domain.Contracts;
using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Infrastructure.Persistence;
using FSH.Starter.WebApi.Catalog.Infrastructure.Tests.Persistence;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Infrastructure.Tests.Persistence.Repositories;

[Collection("CatalogDbContextTests")]
public class BrandRepositoryTests : RepositoryTestBase<Brand, Guid>
{
    public BrandRepositoryTests(CatalogDbContextFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task AddAsync_ShouldAddBrandToDatabase()
    {
        // Arrange
        var brand = new Brand("Test Brand", "Test Description");

        // Act
        await Repository.AddAsync(brand);
        await SaveChangesAsync();

        // Assert
        var result = await Repository.GetByIdAsync(brand.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be(brand.Name);
        result.Description.Should().Be(brand.Description);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingBrand()
    {
        // Arrange
        var brand = new Brand("Original Name", "Original Description");
        await Repository.AddAsync(brand);
        await SaveChangesAsync();

        // Act
        brand.Update("Updated Name", "Updated Description");
        await Repository.UpdateAsync(brand);
        await SaveChangesAsync();

        // Assert
        var result = await Repository.GetByIdAsync(brand.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveBrandFromDatabase()
    {
        // Arrange
        var brand = new Brand("Brand to Delete", "Will be deleted");
        await Repository.AddAsync(brand);
        await SaveChangesAsync();

        // Act
        await Repository.DeleteAsync(brand);
        await SaveChangesAsync();

        // Assert
        var result = await Repository.GetByIdAsync(brand.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullForNonExistentId()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await Repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ListAsync_ShouldReturnAllBrands()
    {
        // Arrange
        var brands = new List<Brand>
        {
            new("Brand 1", "Description 1"),
            new("Brand 2", "Description 2"),
            new("Brand 3", "Description 3")
        };

        foreach (var brand in brands)
        {
            await Repository.AddAsync(brand);
        }
        await SaveChangesAsync();

        // Act
        var result = await Repository.ListAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Select(b => b.Name).Should().Contain(new[] { "Brand 1", "Brand 2", "Brand 3" });
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var brands = new List<Brand>
        {
            new("Brand 1", "Description 1"),
            new("Brand 2", "Description 2")
        };

        foreach (var brand in brands)
        {
            await Repository.AddAsync(brand);
        }
        await SaveChangesAsync();

        // Act
        var count = await Repository.CountAsync();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnTrueForExistingBrand()
    {
        // Arrange
        var brand = new Brand("Test Brand", "Test Description");
        await Repository.AddAsync(brand);
        await SaveChangesAsync();

        // Act
        var exists = await Repository.AnyAsync(b => b.Name == "Test Brand");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnFalseForNonExistingBrand()
    {
        // Act
        var exists = await Repository.AnyAsync(b => b.Name == "Non Existing Brand");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ShouldReturnFirstMatchingBrand()
    {
        // Arrange
        var brands = new List<Brand>
        {
            new("Apple", "Tech Company"),
            new("Samsung", "Tech Company"),
            new("Nike", "Sportswear")
        };

        foreach (var brand in brands)
        {
            await Repository.AddAsync(brand);
        }
        await SaveChangesAsync();

        // Act
        var result = await Repository.FirstOrDefaultAsync(b => b.Description == "Tech Company");

        // Assert
        result.Should().NotBeNull();
        result!.Description.Should().Be("Tech Company");
    }
}
