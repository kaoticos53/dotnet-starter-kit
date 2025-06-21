using Ardalis.Specification;
using Catalog.Infrastructure.IntegrationTests.Fixtures;
using Catalog.Infrastructure.IntegrationTests.TestData;
using FSH.Framework.Core.Domain.Contracts;
using FSH.Framework.Core.Persistence;
using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Catalog.Infrastructure.IntegrationTests.Repositories;

/// <summary>
/// Contiene pruebas de integración para el repositorio de Brand.
/// </summary>
public class BrandRepositoryTests : BaseIntegrationTest
{
    private readonly CatalogRepository<Brand> _repository;
    private readonly ITestOutputHelper _output;

    public BrandRepositoryTests(TestDatabaseFixture fixture, ITestOutputHelper output) 
        : base(fixture)
    {
        _output = output;
        _repository = new CatalogRepository<Brand>(Context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddBrandToDatabase()
    {
        // Arrange
        var brand = TestDataBuilder.BuildBrand();

        // Act
        await _repository.AddAsync(brand);
        await Context.SaveChangesAsync();

        // Assert
        var savedBrand = await Context.Brands.FindAsync(brand.Id);
        savedBrand.Should().NotBeNull();
        savedBrand!.Name.Should().Be(brand.Name);
        savedBrand.Description.Should().Be(brand.Description);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnBrand_WhenBrandExists()
    {
        // Arrange
        var brand = TestDataBuilder.BuildBrand();
        await Context.Brands.AddAsync(brand);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(brand.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(brand.Id);
        result.Name.Should().Be(brand.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenBrandDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ListAsync_ShouldReturnAllBrands()
    {
        // Arrange
        var brands = TestDataBuilder.BuildBrands(3);
        await Context.Brands.AddRangeAsync(brands);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.ListAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Select(b => b.Id).Should().Contain(brands.Select(b => b.Id));
    }

    // Replace the instantiation of the Brand object in the UpdateAsync_ShouldUpdateBrandInDatabase test method  
    // with the static Create method provided by the Brand class.  

    [Fact]
    public async Task UpdateAsync_ShouldUpdateBrandInDatabase()
    {
        // Arrange  
        Brand brand = TestDataBuilder.BuildBrand();
        await Context.Brands.AddAsync(brand);
        await Context.SaveChangesAsync();

        // Detach the entity to simulate a new context  
        Context.Entry(brand).State = EntityState.Detached;

        // Act  
        var updatedBrand = brand.Update(
            "Updated " + brand.Name,
            "Updated " + brand.Description
        );

        // Ensure the ID remains the same  
        await _repository.UpdateAsync(updatedBrand);
        await Context.SaveChangesAsync();

        // Assert  
        var result = await Context.Brands.FindAsync(brand.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be(brand.Name);
        result.Description.Should().Be(brand.Description);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveBrandFromDatabase()
    {
        // Arrange
        var brand = TestDataBuilder.BuildBrand();
        await Context.Brands.AddAsync(brand);
        await Context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(brand);
        await Context.SaveChangesAsync();

        // Assert
        var result = await Context.Brands.FindAsync(brand.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var brands = TestDataBuilder.BuildBrands(5);
        await Context.Brands.AddRangeAsync(brands);
        await Context.SaveChangesAsync();

        // Act
        var count = await _repository.CountAsync();

        // Assert
        count.Should().Be(5);
    }
}
