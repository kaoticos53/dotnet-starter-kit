using FluentValidation.TestHelper;
using FSH.Starter.WebApi.Catalog.Application.Brands.Create.v1;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Brands.Create.v1;

public class CreateBrandCommandValidatorTests
{
    private readonly CreateBrandCommandValidator _validator;

    public CreateBrandCommandValidatorTests()
    {
        _validator = new CreateBrandCommandValidator();
    }

    [Theory]
    [InlineData("Valid Name")]
    [InlineData("A")] // Minimum length
    [InlineData("This is a very long brand name that is exactly 100 characters long to test the maximum length validation for brand names")] // Maximum length
    public void Validate_WithValidName_ShouldNotHaveValidationError(string name)
    {
        // Arrange
        var command = new CreateBrandCommand(name, "Valid Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Validate_WithInvalidName_ShouldHaveValidationError(string invalidName)
    {
        // Arrange
        var command = new CreateBrandCommand(invalidName, "Valid Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithNameExceedingMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('a', 101); // 101 characters
        var command = new CreateBrandCommand(longName, "Valid Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode("MaximumLengthValidator");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Valid Description")]
    [InlineData("Short")]
    [InlineData("This is a very long description that is exactly 1000 characters long. " +
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam auctor, " +
                "nisl eget ultricies tincidunt, nunc nisl aliquam nunc, vitae aliquam " +
                "nisl nunc vitae nisl. Sed vitae nisl eget nisl aliquam aliquam. " +
                "Nullam auctor, nisl eget ultricies tincidunt, nunc nisl aliquam nunc, " +
                "vitae aliquam nisl nunc vitae nisl. Sed vitae nisl eget nisl aliquam " +
                "aliquam. Nullam auctor, nisl eget ultricies tincidunt, nunc nisl " +
                "aliquam nunc, vitae aliquam nisl nunc vitae nisl. Sed vitae nisl eget " +
                "nisl aliquam aliquam. Nullam auctor, nisl eget ultricies tincidunt, " +
                "nunc nisl aliquam nunc, vitae aliquam nisl nunc vitae nisl. Sed vitae " +
                "nisl eget nisl aliquam aliquam. Nullam auctor, nisl eget ultricies " +
                "tincidunt, nunc nisl aliquam nunc, vitae aliquam nisl nunc vitae nisl. " +
                "Sed vitae nisl eget nisl aliquam aliquam. Nullam auctor, nisl eget " +
                "ultricies tincidunt, nunc nisl aliquam nunc, vitae aliquam nisl nunc " +
                "vitae nisl. Sed vitae nisl eget nisl aliquam aliquam. Nullam auctor, " +
                "nisl eget ultricies tincidunt, nunc nisl aliquam nunc, vitae aliquam " +
                "nisl nunc vitae nisl. Sed vitae nisl eget nisl aliquam aliquam.")] // 1000 characters
    public void Validate_WithValidDescription_ShouldNotHaveValidationError(string description)
    {
        // Arrange
        var command = new CreateBrandCommand("Valid Name", description);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithDescriptionExceedingMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longDescription = new string('a', 1001); // 1001 characters
        var command = new CreateBrandCommand("Valid Name", longDescription);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorCode("MaximumLengthValidator");
    }
}
