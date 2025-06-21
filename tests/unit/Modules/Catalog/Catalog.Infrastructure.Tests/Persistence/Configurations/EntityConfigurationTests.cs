using FSH.Starter.WebApi.Catalog.Domain;
using FSH.Starter.WebApi.Catalog.Infrastructure.Persistence;
using FSH.Starter.WebApi.Catalog.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Infrastructure.Tests.Persistence.Configurations;

[Collection("CatalogDbContextTests")]
public class EntityConfigurationTests : IClassFixture<CatalogDbContextFixture>
{
    private readonly CatalogDbContext _context;

    public EntityConfigurationTests(CatalogDbContextFixture fixture)
    {
        _context = fixture.CreateContext();
    }

    [Fact]
    public void BrandConfiguration_ShouldHaveCorrectTableName()
    {
        // Arrange
        var entityType = _context.Model.FindEntityType(typeof(Brand));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("Brands");
    }

    [Fact]
    public void BrandConfiguration_ShouldHaveCorrectPrimaryKey()
    {
        // Arrange
        var entityType = _context.Model.FindEntityType(typeof(Brand));
        var primaryKey = entityType?.FindPrimaryKey();

        // Assert
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().HaveCount(1);
        primaryKey.Properties[0].Name.Should().Be("Id");
    }

    [Fact]
    public void BrandConfiguration_ShouldHaveRequiredName()
    {
        // Arrange
        var entityType = _context.Model.FindEntityType(typeof(Brand));
        var nameProperty = entityType?.FindProperty("Name");

        // Assert
        nameProperty.Should().NotBeNull();
        nameProperty!.IsNullable.Should().BeFalse();
        nameProperty.GetMaxLength().Should().Be(100);
    }

    [Fact]
    public void ProductConfiguration_ShouldHaveCorrectTableName()
    {
        // Arrange
        var entityType = _context.Model.FindEntityType(typeof(Product));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("Products");
    }

    [Fact]
    public void ProductConfiguration_ShouldHaveCorrectPrimaryKey()
    {
        // Arrange
        var entityType = _context.Model.FindEntityType(typeof(Product));
        var primaryKey = entityType?.FindPrimaryKey();

        // Assert
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().HaveCount(1);
        primaryKey.Properties[0].Name.Should().Be("Id");
    }

    [Fact]
    public void ProductConfiguration_ShouldHaveRequiredName()
    {
        // Arrange
        var entityType = _context.Model.FindEntityType(typeof(Product));
        var nameProperty = entityType?.FindProperty("Name");

        // Assert
        nameProperty.Should().NotBeNull();
        nameProperty!.IsNullable.Should().BeFalse();
        nameProperty.GetMaxLength().Should().Be(100);
    }

    [Fact]
    public void ProductConfiguration_ShouldHavePriceWithPrecision()
    {
        // Arrange
        var entityType = _context.Model.FindEntityType(typeof(Product));
        var priceProperty = entityType?.FindProperty("Price");

        // Assert
        priceProperty.Should().NotBeNull();
        priceProperty!.GetPrecision().Should().Be(18);
        priceProperty.GetScale().Should().Be(2);
    }

    [Fact]
    public void ProductConfiguration_ShouldHaveBrandRelationship()
    {
        // Arrange
        var entityType = _context.Model.FindEntityType(typeof(Product));
        var brandFk = entityType?.FindNavigation("Brand")?.ForeignKey;

        // Assert
        brandFk.Should().NotBeNull();
        brandFk!.PrincipalEntityType.ClrType.Should().Be(typeof(Brand));
        brandFk.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);
    }

    [Fact]
    public void ProductConfiguration_ShouldHaveAuditableProperties()
    {
        // Arrange
        var entityType = _context.Model.FindEntityType(typeof(Product));
        var createdByProperty = entityType?.FindProperty("CreatedBy");
        var createdOnProperty = entityType?.FindProperty("CreatedOn");
        var lastModifiedByProperty = entityType?.FindProperty("LastModifiedBy");
        var lastModifiedOnProperty = entityType?.FindProperty("LastModifiedOn");

        // Assert
        createdByProperty.Should().NotBeNull();
        createdByProperty!.IsNullable.Should().BeFalse();
        createdByProperty.GetMaxLength().Should().Be(128);

        createdOnProperty.Should().NotBeNull();
        createdOnProperty!.IsNullable.Should().BeFalse();

        lastModifiedByProperty.Should().NotBeNull();
        lastModifiedByProperty!.IsNullable.Should().BeTrue();
        lastModifiedByProperty.GetMaxLength().Should().Be(128);

        lastModifiedOnProperty.Should().NotBeNull();
        lastModifiedOnProperty!.IsNullable.Should().BeTrue();
    }
}
