using Xunit;
using FSH.Starter.WebApi.Catalog.Domain;

namespace Catalog.Domain.UnitTests.Tests;

public class SimpleBrandTest
{
    [Fact]
    public void SimpleBrandTest_Create_ShouldWork()
    {
        // Arrange
        var name = "Test Brand";
        var description = "Test Description";

        // Act
        var brand = Brand.Create(name, description);

        // Assert
        Assert.NotNull(brand);
        Assert.Equal(name, brand.Name);
        Assert.Equal(description, brand.Description);
    }
}
