using FluentValidation.TestHelper;
using FSH.Starter.WebApi.Catalog.Application.Brands.Update.v1;
using Xunit;

namespace FSH.Starter.WebApi.Catalog.Application.Tests.Brands.Update.v1;

public class UpdateBrandCommandValidatorTests
{
    private readonly UpdateBrandCommandValidator _validator;

    public UpdateBrandCommandValidatorTests()
    {
        _validator = new UpdateBrandCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateBrandCommand(Guid.NewGuid(), "Valid Name", "Valid Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullName_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateBrandCommand(Guid.NewGuid(), null, "Valid Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("a")] // Less than minimum length
    public void Validate_WithInvalidName_ShouldHaveValidationError(string invalidName)
    {
        // Arrange
        var command = new UpdateBrandCommand(Guid.NewGuid(), invalidName, "Valid Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameExceedingMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('a', 101); // 101 characters
        var command = new UpdateBrandCommand(Guid.NewGuid(), longName, "Valid Description");

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
        var command = new UpdateBrandCommand(Guid.NewGuid(), "Valid Name", description);

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
        var command = new UpdateBrandCommand(Guid.NewGuid(), "Valid Name", longDescription);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorCode("MaximumLengthValidator");
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateBrandCommand(Guid.Empty, "Valid Name", "Valid Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}
