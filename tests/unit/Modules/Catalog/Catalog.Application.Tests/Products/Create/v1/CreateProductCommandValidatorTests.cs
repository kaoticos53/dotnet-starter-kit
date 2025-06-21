using FluentValidation.TestHelper;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Products.Create.v1;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator;

    public CreateProductCommandValidatorTests()
    {
        _validator = new CreateProductCommandValidator();
    }

    [Theory]
    [InlineData("Valid Name", 1.00)]
    [InlineData("AB", 0.01)] // Minimum length and price
    [InlineData("A very long product name that is exactly 75 characters long to test the maximum length validation", 999999.99)]
    public void Validate_WithValidData_ShouldNotHaveValidationError(string name, decimal price)
    {
        // Arrange
        var command = new CreateProductCommand(name, price, "Valid Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")] // Less than minimum length (2)
    public void Validate_WithInvalidName_ShouldHaveValidationError(string invalidName)
    {
        // Arrange
        var command = new CreateProductCommand(invalidName, 10.00m, "Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameExceedingMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('A', 76); // 76 characters (max is 75)
        var command = new CreateProductCommand(longName, 10.00m, "Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode("MaximumLengthValidator");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Validate_WithInvalidPrice_ShouldHaveValidationError(decimal invalidPrice)
    {
        // Arrange
        var command = new CreateProductCommand("Valid Name", invalidPrice, "Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorCode("GreaterThanValidator");
    }

    [Fact]
    public void Validate_WithNullDescription_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateProductCommand("Valid Name", 10.00m, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithEmptyDescription_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateProductCommand("Valid Name", 10.00m, string.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }


    [Fact]
    public void Validate_WithNullBrandId_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateProductCommand("Valid Name", 10.00m, "Description", null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BrandId);
    }

    [Fact]
    public void Validate_WithEmptyBrandId_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateProductCommand("Valid Name", 10.00m, "Description", Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BrandId);
    }

    [Fact]
    public void Validate_WithValidBrandId_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateProductCommand("Valid Name", 10.00m, "Description", Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BrandId);
    }

    [Fact]
    public void Validate_WithMultipleValidationErrors_ReturnsAllErrors()
    {
        // Arrange
        var command = new CreateProductCommand("", -1.00m);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.Errors.Should().HaveCount(2);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }
}
