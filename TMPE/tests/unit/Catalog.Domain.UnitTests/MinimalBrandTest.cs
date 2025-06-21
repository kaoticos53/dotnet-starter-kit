using Xunit;
using FSH.Starter.WebApi.Catalog.Domain;

namespace MinimalTest;

public class MinimalBrandTest
{
    [Fact]
    public void MinimalBrandTest_Create_ShouldWork()
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
