using FluentValidation.TestHelper;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Products.Update.v1;

public class UpdateProductCommandValidatorTests
{
    private readonly UpdateProductCommandValidator _validator;
    private readonly Guid _testId = Guid.NewGuid();
    private const string ValidName = "Valid Product Name";
    private const decimal ValidPrice = 9.99m;

    public UpdateProductCommandValidatorTests()
    {
        _validator = new UpdateProductCommandValidator();
    }

    [Theory]
    [InlineData(ValidName, 0.01)]
    [InlineData("AB", 1.00)] // Minimum length for name
    [InlineData("A very long product name that is exactly 75 characters long to test the maximum length validation", 999999.99)]
    [InlineData(ValidName, 1.00)]
    public void Validate_WithValidData_ShouldNotHaveValidationError(string name, decimal price)
    {
        // Arrange
        var command = new UpdateProductCommand(_testId, name, price, "Valid Description");

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
        var command = new UpdateProductCommand(_testId, invalidName, ValidPrice, "Description");

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
        var command = new UpdateProductCommand(_testId, longName, ValidPrice, "Description");

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
        var command = new UpdateProductCommand(_testId, ValidName, invalidPrice, "Description");

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
        var command = new UpdateProductCommand(_testId, ValidName, ValidPrice, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithEmptyDescription_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand(_testId, ValidName, ValidPrice, string.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithNullBrandId_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand(_testId, ValidName, ValidPrice, "Description", null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BrandId);
    }

    [Fact]
    public void Validate_WithEmptyBrandId_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand(_testId, ValidName, ValidPrice, "Description", Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BrandId);
    }

    [Fact]
    public void Validate_WithValidBrandId_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand(_testId, ValidName, ValidPrice, "Description", Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BrandId);
    }

    [Fact]
    public void Validate_WithMultipleValidationErrors_ReturnsAllErrors()
    {
        // Arrange
        var command = new UpdateProductCommand(Guid.Empty, "", -1.00m);

        // Act
        var result = _validator.TestValidate(command);

        // Assert - Should have errors for empty name and invalid price
        // Note: Empty GUID is valid for ID, so we don't validate that
        result.Errors.Should().HaveCount(2);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }
}
